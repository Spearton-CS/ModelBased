using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;

    /// <summary>
    /// Custom collection, called as 'ShadowStack', used by <see cref="ModelPool{TModel, TID}"/>.
    /// Its used for a stack of fixed-size elements <typeparamref name="TModel"/>, where when overflowing, the 'old' elements are deleted and replaced with new ones.
    /// Its possible to "return" items from this stack.
    /// </summary>
    [DebuggerDisplay("Count = {Count} [Capacity = {Capacity}]")]
    public class PoolShadowStack<TModel, TID> : IPoolShadowStack<PoolShadowStack<TModel, TID>, TModel, TID>
        where TID : notnull
        where TModel : notnull, IDataModel<TID>
    {
        #region Protected

        /// <summary>
        /// Array of <typeparamref name="TModel"/>, which have Age ('Old' item). This an index, which increases at each <see cref="models"/> modification
        /// </summary>
        protected (int Old, TModel? Model)[]? models = null;
        /// <summary>
        /// Count of modifications of <see cref="models"/>. Used by <see cref="GetEnumerator()"/> to validate data between yield returns
        /// </summary>
        protected ulong version = 0;
        /// <summary>
        /// Count of items, where old >= 0
        /// </summary>
        protected volatile int count = 0;
        /// <summary>
        /// RW lock. Supports async
        /// </summary>
        protected SemaphoreSlim semaphore = new(1, 1);

        #endregion

        #region Constructing

        /// <summary>
        /// Constructs empty instance, but recommended to use <see cref="Empty"/>, which shared
        /// </summary>
        [Obsolete("Constructs empty instance, but recommended to use <see cref=\"Empty\"/>, which shared")]
        public PoolShadowStack()
        {

        }

        /// <summary>
        /// Constructs this <see cref="PoolShadowStack{TModel, TID}"/> with <paramref name="capacity"/>.
        /// 0 or less will init empty instance, but recommended to use <see cref="Empty"/>, which shared
        /// </summary>
        /// <param name="capacity"></param>
        public PoolShadowStack(int capacity)
        {
            if (capacity > 0)
            {
                models = new (int, TModel)[capacity];
                for (int i = 0; i < capacity; i++)
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                    models[i] = (-1, default);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                /*
                 * Default is OK, bc we always must check Old > -1 (at least 0, newest or older).
                 * We dont mark Model as nullable bc its default (can be null) only if Old <= -1
                 */
            }
        }

        #endregion

        #region Shared

#pragma warning disable CS0618 // obsolete attribute ignoring
        /// <inheritdoc/>
        public static PoolShadowStack<TModel, TID> Empty { get; } = new();
#pragma warning restore CS0618 // obsolete attribute ignoring

        #endregion

        #region Counts

        /// <inheritdoc/>
        public virtual int Capacity => models?.Length ?? 0;
        /// <inheritdoc/>
        public virtual int Count
        {
            get => count;
            protected set => count = value;
        }

        #endregion

        #region Push

        /// <summary>
        /// Core for <see cref="Push"/>, <see cref="PushAsync"/>
        /// </summary>
        /// <param name="model"></param>
        protected virtual void PushCore(in TModel model)
        {
            for (int i = 0; i < models!.Length; i++)
            {
                var (Old, Model) = models[i];
                if (Old > -1 && model.EqualsByID(Model.ID)) //model is already exist, lets 'refresh' its old and make older others
                {
                    for (int j = 0; j < models.Length; j++)
                        if (j != i && models[j].Old > -1) //All models, excluding model i
                            models[j].Old++; //We always must old up models in Push method if we modify sth
                    models[i].Old = 0;
                    Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                    return;
                }
            }
            int freeIndex = -1, oldestIndex = -1, oldest = -1; //model isn't exist, that fields will help us, if no space in models - we will replace oldest.

            for (int i = 0; i < models.Length; i++) //Lets check all models. We need to try find free index, oldestIndex and oldest (item)
            {
                int old = models[i].Old; //Save as local, bc we use it 3+ times
                if (old > -1) //Its not empty index
                {
                    models[i].Old++; //Add old BEFORE COMPARE with oldest, bc all models will old up when adding new one
                    if (old > oldest) //This older than oldest
                    {
                        oldest = old;
                        oldestIndex = i; //Set it, bc if no space in models - we will replace this by new one
                    }
                }
                else if (freeIndex == -1) //Its empty index and freeIndex not set check
                    freeIndex = i;
            }

            if (freeIndex != -1) //We dont need to replace oldest one
            {
                models[freeIndex] = (0, model);
                Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                Interlocked.Increment(ref count); // We added model, increase stat
            }
            else //We need to replace oldest one
            {
                models[oldestIndex] = (0, model);
                Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
            }
        }

        /// <inheritdoc/>
        public virtual void Push(TModel model, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                semaphore.Wait(token);
                try
                {
                    PushCore(in model);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task PushAsync(TModel model, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    PushCore(in model);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void PushMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(models, nameof(models));
            if (this.models is not null && this.models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                semaphore.Wait(token);
                try
                {
                    foreach (TModel model in models)
                    {
                        PushCore(in model);
                        token.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task PushManyAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(models, nameof(models));
            if (this.models is not null && this.models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    foreach (TModel model in models)
                    {
                        PushCore(in model);
                        token.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task PushManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(models, nameof(models));
            if (this.models is not null && this.models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    await foreach (TModel model in models.WithCancellation(token))
                        PushCore(in model);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        #endregion

        #region Pop

        /// <summary>
        /// Core for <see cref="TryPop(TID, CancellationToken)"/>, <see cref="TryPopAsync(TID, CancellationToken)"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual TModel? PopCore(in TID id)
        {
            for (int i = 0; i < models!.Length; i++)
            {
                var (Old, Model) = models[i];
                if (Old > -1 && Model.EqualsByID(id))
                {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                    models[i] = (-1, default); //This index is cleaned now, we dont need replace oldest item by new
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                    /*
                     * Default is OK, bc we always must check Old > -1 (at least 0, newest or older).
                     * We dont mark Model as nullable bc its default (can be null) only if Old <= -1
                     */

                    for (int j = 0; j < models.Length; j++)
                        if (j != i && models[j].Old > -1) //All models, excluding model i
                            models[j].Old++; //We always must old up models in Pop method if we modify sth

                    Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                    Interlocked.Decrement(ref count); // We removed model, decrease stat
                    return Model; //We found the model with id
                }
            }
            return default; //We didn't find the model with id
        }

        /// <inheritdoc/>
        public virtual TModel? TryPop(TID id, CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                semaphore.Wait(token);
                try
                {
                    return PopCore(in id);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
                return default;
        }

        /// <inheritdoc/>
        public virtual async Task<TModel?> TryPopAsync(TID id, CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    return PopCore(in id);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
                return default;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TModel?> TryPopMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                semaphore.Wait(token);
                try
                {
                    foreach (TID id in ids)
                    {
                        yield return PopCore(in id);
                        token.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<TModel?> TryPopManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    foreach (TID id in ids)
                    {
                        yield return PopCore(in id);
                        token.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<TModel?> TryPopManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    await foreach (TID id in ids.WithCancellation(token))
                        yield return PopCore(in id);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        #endregion

        #region Pop no specific ID

        /// <summary>
        /// Core for <see cref="TryPop(CancellationToken)"/>, <see cref="TryPopAsync(CancellationToken)"/>
        /// </summary>
        /// <returns></returns>
        protected virtual TModel? PopCore()
        {
            (int Old, TModel? Model) oldest = (-1, default);
            for (int i = 0; i < models!.Length; i++)
            {
                var model = models[i];
                if (model.Old > oldest.Old)
                    oldest = model;
            }
            return oldest.Old >= 0
                ? oldest.Model
                : default;
        }

        /// <inheritdoc/>
        public virtual TModel? TryPop(CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                semaphore.Wait(token);
                try
                {
                    return PopCore();
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
                return default;

        }
        
        /// <inheritdoc/>
        public virtual async Task<TModel?> TryPopAsync(CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    return PopCore();
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
                return default;
        }

        #endregion

        #region Searching

        /// <summary>
        /// Core for <see cref="Contains"/> | <see cref="ContainsAsync"/>, <see cref="ContainsMany"/>,
        /// <see cref="ContainsManyAsync(IAsyncEnumerable{TID}, CancellationToken)"/> | <see cref="ContainsManyAsync(IEnumerable{TID}, CancellationToken)"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual bool ContainsCore(in TID id) //in bc we dont know size of TID, and maybe 
        {
            for (int i = 0; i < models!.Length; i++)
            {
                var (Old, Model) = models[i];
                if (Old > -1 && Model.EqualsByID(id))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public virtual bool Contains(TID id, CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                semaphore.Wait(token);
                try
                {
                    return ContainsCore(in id);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
                return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ContainsAsync(TID id, CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    return ContainsCore(in id);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
                return false;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                semaphore.Wait(token);
                try
                {
                    foreach (TID id in ids)
                        yield return ContainsCore(in id);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    foreach (TID id in ids)
                        yield return ContainsCore(in id);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    await foreach (TID id in ids.WithCancellation(token))
                        yield return ContainsCore(in id);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        #endregion

        #region Additionals (clear)

        /// <summary>
        /// Core for <see cref="Clear"/> and <see cref="ClearAsync"/>
        /// </summary>
        /// <returns></returns>
        protected virtual int ClearCore()
        {
            int cleaned = 0;
            for (int i = 0; i < models!.Length; i++)
                if (models[i].Old > -1)
                {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                    models[i] = (-1, default);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                    /*
                     * Default is OK, bc we always must check Old > -1 (at least 0, newest or older).
                     * We dont mark Model as nullable bc its default (can be null) only if Old <= -1
                     */
                    cleaned++;
                }
            if (cleaned > 0)
            {
                Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                Interlocked.Add(ref count, -cleaned); // We removed <cleaned> models, decrease stat
            }
            return cleaned;
        }

        /// <inheritdoc/>
        public virtual async Task<int> ClearAsync(CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    return ClearCore();
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
                return 0;

        }

        /// <inheritdoc/>
        public virtual int Clear(CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                semaphore.Wait(token);
                try
                {
                    return ClearCore();
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
                return 0;
        }

        #endregion

        #region Enumeration

        /// <inheritdoc/>
        public virtual async IAsyncEnumerator<TModel> GetAsyncEnumerator(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (models is not null && models.Length > 0)
            {
                ulong iterationVersion = Interlocked.Read(in version);
                for (int i = 0; i < models.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    TModel model = default; //Use default bc analyzer and compiler cant 100% be sure, that in lock models we will always set model, or set skip to true
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    bool skip = false;
                    await semaphore.WaitAsync(token);
                    try
                    {
                        ulong ver = Interlocked.Read(in version);
                        if (iterationVersion != ver)
                            throw new InvalidDataException($"PoolActiveStack version mismatch. Iteration version: {iterationVersion}; Data version: {ver}");
                        else if (models[i].Old > -1)
                        {
                            model = models[i].Model; //Copy to local bc we must end locking of models (e.g: parallel Threads called GetEnumerator both)
                            skip = false;
                        }
                        else
                            skip = true;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                    if (!skip)
#pragma warning disable CS8603 // Possible null reference return.
                        yield return model;
#pragma warning restore CS8603 // Possible null reference return.
                }
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerator<TModel> GetEnumerator(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (models is not null && models.Length > 0)
            {
                ulong iterationVersion = Interlocked.Read(in version);
                for (int i = 0; i < models.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    TModel model = default; //Use default bc analyzer and compiler cant 100% be sure, that in lock models we will always set model, or set skip to true
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    bool skip = false;
                    semaphore.Wait(token);
                    try
                    {
                        ulong ver = Interlocked.Read(in version);
                        if (iterationVersion != ver)
                            throw new InvalidDataException($"PoolActiveStack version mismatch. Iteration version: {iterationVersion}; Data version: {ver}");
                        else if (models[i].Old > -1)
                        {
                            model = models[i].Model; //Copy to local bc we must end locking of models (e.g: parallel Threads called GetEnumerator both)
                            skip = false;
                        }
                        else
                            skip = true;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                    if (!skip)
#pragma warning disable CS8603 // Possible null reference return.
                        yield return model;
#pragma warning restore CS8603 // Possible null reference return.
                }
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerator<TModel> GetEnumerator() => GetEnumerator(default);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
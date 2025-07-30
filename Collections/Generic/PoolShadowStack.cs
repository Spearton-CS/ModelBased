using System.Collections;
using System.Diagnostics;

namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;
    using System.Reflection;
    using System.Threading.Tasks;

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
        protected (int Old, TModel Model)[]? models = null;
        /// <summary>
        /// Count of modifications of <see cref="models"/>. Used by <see cref="GetEnumerator"/> to validate data between yield returns
        /// </summary>
        protected long version = 0;
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

        /// <inheritdoc/>
#pragma warning disable CS0618 // obsolete attribute ignoring
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
                    for (int i = 0; i < models.Length; i++)
                        if (models[i].Old > -1 && model.EqualsByID(models[i].Model.ID)) //model is already exist, lets 'refresh' its old and make older others
                        {
                            for (int j = 0; j < models.Length; j++)
                                if (j != i && models[j].Old > -1) //All models, excluding model i
                                    models[j].Old++; //We always must old up models in Push method if we modify sth
                            models[i].Old = 0;
                            Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                            return;
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
                    }
                    else //We need to replace oldest one
                    {
                        models[oldestIndex] = (0, model);
                        Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public async Task PushAsync(TModel model, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    for (int i = 0; i < models.Length; i++)
                        if (models[i].Old > -1 && model.EqualsByID(models[i].Model.ID)) //model is already exist, lets 'refresh' its old and make older others
                        {
                            for (int j = 0; j < models.Length; j++)
                                if (j != i && models[j].Old > -1) //All models, excluding model i
                                    models[j].Old++; //We always must old up models in Push method if we modify sth
                            models[i].Old = 0;
                            Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                            return;
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
                    }
                    else //We need to replace oldest one
                    {
                        models[oldestIndex] = (0, model);
                        Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public void PushMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(models, nameof(models));
            if (this.models is not null && this.models.Length > 0)
            {
                semaphore.Wait(token);
                try
                {
                    foreach (TModel model in models)
                    {
                        for (int i = 0; i < this.models.Length; i++)
                            if (this.models[i].Old > -1 && model.EqualsByID(this.models[i].Model.ID)) //model is already exist, lets 'refresh' its old and make older others
                            {
                                for (int j = 0; j < this.models.Length; j++)
                                    if (j != i && this.models[j].Old > -1) //All models, excluding model i
                                        this.models[j].Old++; //We always must old up models in Push method if we modify sth
                                this.models[i].Old = 0;
                                Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                                return;
                            }
                        int freeIndex = -1, oldestIndex = -1, oldest = -1; //model isn't exist, that fields will help us, if no space in models - we will replace oldest.

                        for (int i = 0; i < this.models.Length; i++) //Lets check all models. We need to try find free index, oldestIndex and oldest (item)
                        {
                            int old = this.models[i].Old; //Save as local, bc we use it 3+ times
                            if (old > -1) //Its not empty index
                            {
                                this.models[i].Old++; //Add old BEFORE COMPARE with oldest, bc all models will old up when adding new one
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
                            this.models[freeIndex] = (0, model);
                            Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                        }
                        else //We need to replace oldest one
                        {
                            this.models[oldestIndex] = (0, model);
                            Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public Task PushManyAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task PushManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Pop

        /// <inheritdoc/>
        public virtual TModel? Pop(TID id, CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                semaphore.Wait(token);
                try
                {
                    for (int i = 0; i < models.Length; i++)
                    {
                        if (models[i].Old > -1 && models[i].Model.EqualsByID(id))
                        {
                            TModel model = models[i].Model;
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
                            return model; //We found the model with id
                        }
                    }
                    return default; //We didn't find the model with id
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
        public Task<TModel?> PopAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<TModel?> PopMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TModel?> PopManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Searching

        /// <inheritdoc/>
        public bool Contains(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> ContainsAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Additionals (clear, to array)

        /// <inheritdoc/>
        public async Task<int> ClearAsync(CancellationToken token = default)
        {
            if (models is not null && models.Length > 0)
            {
                token.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(token);
                try
                {
                    int cleaned = 0;
                    for (int i = 0; i < models.Length; i++)
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
                        Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                    return cleaned;
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
                    int cleaned = 0;
                    for (int i = 0; i < models.Length; i++)
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
                        Interlocked.Increment(ref version); //We modified models, increase for GetEnumerator validation
                    return cleaned;
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
        public async Task<int> ToArrayAsync(TModel[] array, int index = 0, int count = -1, CancellationToken token = default)
        {
            if (models is null || models.Length == 0 || token.IsCancellationRequested)
                return 0;

            ArgumentNullException.ThrowIfNull(array, nameof(array));

            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, array.Length, nameof(index));

            count = Math.Min(count, Math.Min(models.Length, array.Length - index));
            if (count > 0)
            {
                await semaphore.WaitAsync(token);
                try
                {
                    int copy = 0;
                    for (int i = 0, j = index;
                        i < models.Length
                        && j < array.Length
                        && copy < count
                        && !token.IsCancellationRequested;
                        i++)
                    {
                        if (models[i].Old > -1)
                        {
                            array[j] = models[i].Model;
                            j++;
                            copy++;
                        }
                    }

                    return copy;
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
        public virtual int ToArray(TModel[] array, int index = 0, int count = -1, CancellationToken token = default)
        {
            if (models is null || models.Length == 0 || token.IsCancellationRequested)
                return 0;

            ArgumentNullException.ThrowIfNull(array, nameof(array));

            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, array.Length, nameof(index));

            count = Math.Min(count, Math.Min(models.Length, array.Length - index));
            if (count > 0)
            {
                semaphore.Wait(token);
                try
                {
                    int copy = 0;
                    for (int i = 0, j = index;
                        i < models.Length
                        && j < array.Length
                        && copy < count
                        && !token.IsCancellationRequested;
                        i++)
                    {
                        if (models[i].Old > -1)
                        {
                            array[j] = models[i].Model;
                            j++;
                            copy++;
                        }
                    }

                    return copy;
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
            if (models is not null && models.Length > 0)
            {
                long iterationVersion = Interlocked.Read(in version);
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
                        if (iterationVersion != version)
                            throw new InvalidDataException($"PoolShadowStack version mismatch. Iteration version: {iterationVersion}; Data version: {version}");
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
        public virtual IEnumerator<TModel> GetEnumerator()
        {
            if (models is not null && models.Length > 0)
            {
                semaphore.Wait();
                long iterationVersion = Interlocked.Read(in version);
                semaphore.Release();
                for (int i = 0; i < models.Length; i++)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    TModel model = default; //Use default bc analyzer and compiler cant 100% be sure, that in lock models we will always set model, or set skip to true
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    bool skip = false;
                    semaphore.Wait();
                    try
                    {
                        if (iterationVersion != version)
                            throw new InvalidDataException($"PoolShadowStack version mismatch. Iteration version: {iterationVersion}; Data version: {version}");
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
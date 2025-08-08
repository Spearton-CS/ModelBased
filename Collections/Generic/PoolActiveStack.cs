using System.Collections;
using ModelBased.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModelBased.Collections.Generic
{
    /// <summary>
    /// Pool of active items of <see cref="IModelPool{TModel, TID}"/>
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TID"></typeparam>
    public class PoolActiveStack<TModel, TID> : IPoolActiveStack<TModel, TID>
        where TID : notnull
        where TModel : notnull, IDataModel<TID>
    {
        protected volatile int capacity = 0, count = 0;
        protected long version = 0;

        protected Item firstItem; //Always must be non-null
        protected SemaphoreSlim semaphore = new(1, 1);

        public PoolActiveStack(int itemCapacity = 20)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemCapacity, nameof(itemCapacity));
            ItemCapacity = itemCapacity;
            firstItem = new(itemCapacity);
            capacity = itemCapacity;
        }

        /// <inheritdoc/>
        public virtual int Capacity => capacity;
        /// <inheritdoc/>
        public virtual int Count => count;
        /// <summary>
        /// Count of items (includes uninitialized), which can store <see cref="PoolActiveStack{TModel, TID}.Item"/>
        /// </summary>
        public virtual int ItemCapacity { get; init; }
        protected virtual void IncrementVersion() => Interlocked.Increment(ref version);

        #region Add

        /// <summary>
        /// Add or write operation
        /// </summary>
        /// <param name="model"></param>
        /// <param name="refs"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual int AddCore(TModel model, int refs, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            ((int, TModel?)[] Stack, int Index)? empty = null;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    var (Refs, Model) = current.Stack[i];
                    if (Model is not null)
                    {
                        if (Model.EqualsByID(model.ID))
                        {
                            IncrementVersion();
                            return current.Stack[i].Refs = Refs + refs; //We found 
                        }
                    }
                    else empty ??= (current.Stack, i);
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else if (empty is not null) //We reached the end and have empty slot in stack
                {
                    empty.Value.Stack[empty.Value.Index] = (refs, model);
                    Interlocked.Increment(ref count);
                    IncrementVersion();
                    return refs;
                }
                else //We reached the end and all slots is full
                {
                    current = current.NextItem = new Item(icap);
                    current.Stack[0] = (refs, model);
                    Interlocked.Add(ref capacity, icap);
                    Interlocked.Increment(ref count);
                    IncrementVersion();
                    return refs;
                }
            }
        }

        /// <inheritdoc/>
        public virtual int Add(TModel model, int refs = 1, CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refs, nameof(refs));
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return AddCore(model, refs, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> AddAsync(TModel model, int refs = 1, CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refs, nameof(refs));
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return AddCore(model, refs, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> AddMany(IEnumerable<TModel> models, int refs = 1, CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refs, nameof(refs));
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TModel model in models)
                    yield return AddCore(model, refs, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> AddManyAsync(IEnumerable<TModel> models, int refs = 1, [EnumeratorCancellation] CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refs, nameof(refs));
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TModel model in models)
                    yield return AddCore(model, refs, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> AddManyAsync(IAsyncEnumerable<TModel> models, int refs = 1, [EnumeratorCancellation] CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refs, nameof(refs));
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TModel model in models.WithCancellation(token))
                    yield return AddCore(model, refs, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual void AddManyIgnore(IEnumerable<TModel> models, int refs = 1, CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refs, nameof(refs));
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TModel model in models)
                    AddCore(model, refs, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task AddManyIgnoreAsync(IEnumerable<TModel> models, int refs = 1, CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refs, nameof(refs));
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TModel model in models)
                    AddCore(model, refs, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task AddManyIgnoreAsync(IAsyncEnumerable<TModel> models, int refs = 1, CancellationToken token = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refs, nameof(refs));
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TModel model in models)
                    AddCore(model, refs, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Remove

        /// <summary>
        /// Remove operation
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual bool RemoveCore(TModel model, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    TModel? stackModel = current.Stack[i].Model;
                    if (stackModel is not null && stackModel.EqualsByID(model.ID))
                    {
                        Interlocked.Decrement(ref count);
                        IncrementVersion();
                        current.Stack[i] = (0, default);
                        return true; //We found 
                    }
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Remove operation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual (bool Success, TModel? Model) RemoveCore(TID id, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    TModel? model = current.Stack[i].Model;
                    if (model is not null && model.EqualsByID(id))
                    {
                        Interlocked.Decrement(ref count);
                        IncrementVersion();
                        current.Stack[i] = (0, default);
                        return (true, model); //We found
                    }
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else
                    return (false, default);
            }
        }

        /// <inheritdoc/>
        public virtual (bool Success, TModel? Model) Remove(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return RemoveCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual bool Remove(TModel model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return RemoveCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<(bool Success, TModel? Model)> RemoveAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return RemoveCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> RemoveAsync(TModel model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return RemoveCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<(bool Success, TModel? Model)> RemoveMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TID id in ids)
                    yield return RemoveCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> RemoveMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TModel model in models)
                    yield return RemoveCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(bool Success, TModel? Model)> RemoveManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TID id in ids)
                    yield return RemoveCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> RemoveManyAsync(IEnumerable<TModel> models, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TModel model in models)
                    yield return RemoveCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(bool Success, TModel? Model)> RemoveManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TID id in ids.WithCancellation(token))
                    yield return RemoveCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> RemoveManyAsync(IAsyncEnumerable<TModel> models, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TModel model in models.WithCancellation(token))
                    yield return RemoveCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual void RemoveManyIgnore(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TID id in ids)
                    RemoveCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual void RemoveManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TModel model in models)
                    RemoveCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task RemoveManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TID id in ids)
                    RemoveCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task RemoveManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TModel model in models)
                    RemoveCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task RemoveManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TID id in ids)
                    RemoveCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task RemoveManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TModel model in models)
                    RemoveCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Ref

        /// <summary>
        /// Write operation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual (int Refs, TModel? Model) TryRefCore(TID id, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    TModel? stackModel = current.Stack[i].Model;
                    if (stackModel is not null && stackModel.EqualsByID(id))
                    {
                        IncrementVersion();
                        current.Stack[i].Refs++;
                        return current.Stack[i]; //We found 
                    }
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else
                    return (-1, default);
            }
        }

        /// <summary>
        /// Write operation
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual int TryRefCore(TModel model, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    TModel? stackModel = current.Stack[i].Model;
                    if (stackModel is not null && stackModel.EqualsByID(model.ID))
                    {
                        IncrementVersion();
                        return ++current.Stack[i].Refs; //We found 
                    }
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else
                    return -1;
            }
        }

        /// <inheritdoc/>
        public virtual (int Refs, TModel? Model) TryRef(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return TryRefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual int TryRef(TModel model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return TryRefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<(int Refs, TModel? Model)> TryRefAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return TryRefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> TryRefAsync(TModel model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return TryRefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<(int Refs, TModel? Model)> TryRefMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TID id in ids)
                    yield return TryRefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> TryRefMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TModel model in models)
                    yield return TryRefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(int Refs, TModel? Model)> TryRefManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TID id in ids)
                    yield return TryRefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> TryRefManyAsync(IEnumerable<TModel> models, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TModel model in models)
                    yield return TryRefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(int Refs, TModel? Model)> TryRefManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TID id in ids.WithCancellation(token))
                    yield return TryRefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> TryRefManyAsync(IAsyncEnumerable<TModel> models, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TModel model in models.WithCancellation(token))
                    yield return TryRefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual void TryRefManyIgnore(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TID id in ids)
                    TryRefCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual void TryRefManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TModel model in models)
                    TryRefCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task TryRefManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TID id in ids)
                    TryRefCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task TryRefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TModel model in models)
                    TryRefCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task TryRefManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TID id in ids)
                    TryRefCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task TryRefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TModel model in models)
                    TryRefCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Unref

        /// <summary>
        /// Write or remove operation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual (int Refs, TModel? Model) TryUnrefCore(TID id, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    TModel? stackModel = current.Stack[i].Model;
                    if (stackModel is not null && stackModel.EqualsByID(id))
                    {
                        IncrementVersion();
                        if (--current.Stack[i].Refs > 0) //We found 
                            return current.Stack[i];
                        else
                        {
                            Interlocked.Decrement(ref count);
                            return current.Stack[i] = (-1, default); //Removed
                        }
                    }
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else
                    return (-1, default);
            }
        }

        /// <summary>
        /// Write or remove operation
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual int TryUnrefCore(TModel model, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    TModel? stackModel = current.Stack[i].Model;
                    if (stackModel is not null && stackModel.EqualsByID(model.ID))
                    {
                        IncrementVersion();
                        int refs = --current.Stack[i].Refs;
                        if (refs == 0)
                        {
                            Interlocked.Decrement(ref count);
                            current.Stack[i] = (-1, default); //Removed
                        }
                        return refs; //We found 
                    }
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else
                    return -1;
            }
        }

        /// <inheritdoc/>
        public virtual (int Refs, TModel? Model) TryUnref(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return TryUnrefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual int TryUnref(TModel model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return TryUnrefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<(int Refs, TModel? Model)> TryUnrefAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return TryUnrefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> TryUnrefAsync(TModel model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return TryUnrefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<(int Refs, TModel? Model)> TryUnrefMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TID id in ids)
                    yield return TryUnrefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> TryUnrefMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TModel model in models)
                    yield return TryUnrefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(int Refs, TModel? Model)> TryUnrefManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TID id in ids)
                    yield return TryUnrefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> TryUnrefManyAsync(IEnumerable<TModel> models, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TModel model in models)
                    yield return TryUnrefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(int Refs, TModel? Model)> TryUnrefManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TID id in ids.WithCancellation(token))
                    yield return TryUnrefCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> TryUnrefManyAsync(IAsyncEnumerable<TModel> models, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TModel model in models.WithCancellation(token))
                    yield return TryUnrefCore(model, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual void TryUnrefManyIgnore(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TID id in ids)
                    TryUnrefCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual void TryUnrefManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TModel model in models)
                    TryUnrefCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task TryUnrefManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TID id in ids)
                    TryUnrefCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task TryUnrefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TModel model in models)
                    TryUnrefCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task TryUnrefManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TID id in ids)
                    TryUnrefCore(id, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task TryUnrefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TModel model in models)
                    TryUnrefCore(model, default);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Searching

        /// <summary>
        /// Read-only operation.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual int GetRefsCore(TID id, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    var (Refs, Model) = current.Stack[i];
                    if (Model is not null && Model.EqualsByID(id))
                        return Refs; //We found 
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else
                    return -1;
            }
        }

        /// <inheritdoc/>
        public virtual bool Contains(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return GetRefsCore(id, token) > 0;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ContainsAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return GetRefsCore(id, token) > 0;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TID id in ids)
                    yield return GetRefsCore(id, token) > 0;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TID id in ids)
                    yield return GetRefsCore(id, token) > 0;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TID id in ids.WithCancellation(token))
                    yield return GetRefsCore(id, token) > 0;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual int GetRefs(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return GetRefsCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> GetRefsAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return GetRefsCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> GetRefsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                foreach (TID id in ids)
                    yield return GetRefsCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> GetRefsManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                foreach (TID id in ids)
                    yield return GetRefsCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> GetRefsManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (TID id in ids.WithCancellation(token))
                    yield return GetRefsCore(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Clear

        /// <inheritdoc/>
        public virtual int ClearEmpty(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Item? previous = firstItem, current;
            if (firstItem.NextItem is null) //We must never check first item for empty
                return 0;
            else
                current = firstItem.NextItem;
            int icap = ItemCapacity, removed = 0;
            do
            {
                bool empty = true;
                for (int i = 0; i < icap; i++)
                    if (current.Stack[i].Refs > 0)
                    {
                        empty = false;
                        break;
                    }
                if (empty)
                {
                    removed += icap;
                    Item? next = current.NextItem;

                    current.NextItem = null;
                    if (current is IDisposable disposable)
                        disposable.Dispose();
                    else if (current is IAsyncDisposable adisposable)
                        adisposable.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();

                    previous.NextItem = current = next;
                    if (current is null)
                        return removed;
                }
                else
                {
                    previous = current;
                    current = current.NextItem;
                }
            }
            while (current is not null);
            return removed;
        }

        /// <inheritdoc/>
        public virtual Task<int> ClearEmptyAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Copy

        /// <inheritdoc/>
        [Obsolete("Now we dont need CopyTo - its not implemented")]
        public virtual int CopyTo(TModel[] array, int index = 0, int count = -1, CancellationToken token = default)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        [Obsolete("Now we dont need CopyTo - its not implemented")]
        public virtual Task<int> CopyToAsync(TModel[] array, int index = 0, int count = -1, CancellationToken token = default)
            => throw new NotImplementedException();

        #endregion

        #region Enumeration

        /// <inheritdoc/>
        public virtual IEnumerator<TModel> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Defragmentation

        protected virtual int DefragmentationCore(CancellationToken token = default)
        {
            return -1;
        }

        public virtual int Defragmentation(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return DefragmentationCore(token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public virtual async Task<int> DefragmentationAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return DefragmentationCore(token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Classes

        /// <summary>
        /// Item of this <see cref="PoolActiveStack{TModel, TID}"/>
        /// </summary>
        public class Item
        {
            protected Item() { }
            public Item((int Refs, TModel? Model)[] stack) => Stack = stack;
            public Item(int stackCapacity)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stackCapacity, nameof(stackCapacity));
                Stack = new (int, TModel?)[stackCapacity];
            }

            /// <summary>
            /// Stack of <typeparamref name="TModel"/>s with <see cref="int"/> Refs
            /// </summary>
            public virtual (int Refs, TModel? Model)[] Stack { get; set; }

            /// <summary>
            /// Next <see cref="Item"/> or null
            /// </summary>
            public virtual Item? NextItem { get; set; }
        }

        #endregion
    }
}
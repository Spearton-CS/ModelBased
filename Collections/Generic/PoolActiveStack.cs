using System.Buffers;
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
        protected volatile int capacity = 0, count = 0, enumerationCacheSz = 20;
        protected long version = 0;

        protected Item firstItem; //Always must be non-null
        protected SemaphoreSlim semaphore = new(1, 1);

        protected PoolActiveStack() { }
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
        protected virtual void IncrementCount() => Interlocked.Increment(ref count);
        protected virtual void DecrementCount() => Interlocked.Decrement(ref count);
        protected virtual void IncrementCapacity(int inc) => Interlocked.Add(ref capacity, inc);
        protected virtual void DecrementCapacity(int dec) => Interlocked.Add(ref capacity, -dec);

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
            (Item Item, int Index)? empty = null;
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
                    else empty ??= (current, i);
                }
                if (current.NextItem is not null)
                {
                    current = current.NextItem; //Search in next Item
                    token.ThrowIfCancellationRequested();
                }
                else if (empty is not null) //We reached the end and have empty slot in stack
                {
                    empty.Value.Item.Stack[empty.Value.Index] = (refs, model);
                    empty.Value.Item.IncrementCount();
                    IncrementCount();
                    IncrementVersion();
                    return refs;
                }
                else //We reached the end and all slots is full
                {
                    Item prev = current;
                    current = current.NextItem = new Item(icap);

                    current.Stack[0] = (refs, model);
                    current.Count = 1;
                    current.PrevItem = prev;

                    IncrementCapacity(icap);
                    IncrementCount();
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
                if (current.Count > 0)
                    for (int i = 0; i < icap; i++)
                    {
                        TModel? stackModel = current.Stack[i].Model;
                        if (stackModel is not null && stackModel.EqualsByID(model.ID))
                        {
                            DecrementCount();
                            IncrementVersion();
                            current.Stack[i] = (0, default);
                            current.DecrementCount();
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
                if (current.Count > 0)
                    for (int i = 0; i < icap; i++)
                    {
                        TModel? model = current.Stack[i].Model;
                        if (model is not null && model.EqualsByID(id))
                        {
                            DecrementCount();
                            IncrementVersion();
                            current.Stack[i] = (0, default);
                            current.DecrementCount();
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
                if (current.Count > 0)
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
                if (current.Count > 0)
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
                if (current.Count > 0)
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
                                DecrementCount();
                                current.DecrementCount();
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
                if (current.Count > 0)
                    for (int i = 0; i < icap; i++)
                    {
                        TModel? stackModel = current.Stack[i].Model;
                        if (stackModel is not null && stackModel.EqualsByID(model.ID))
                        {
                            IncrementVersion();
                            int refs = --current.Stack[i].Refs;
                            if (refs == 0)
                            {
                                DecrementCount();
                                current.DecrementCount();
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
                if (current.Count > 0)
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
            semaphore.Wait(token);
            try
            {
                Item? current;
                if (firstItem.NextItem is null) //We must never check first item for empty
                    return 0;
                else
                    current = firstItem.NextItem;
                int icap = ItemCapacity, removed = 0;

                do
                {
                    if (current.Count <= 0)
                    {
                        removed += icap;
                        IncrementVersion();
                        IncrementCapacity(icap);
                        Item? next = current.NextItem, previous = current.PrevItem!; //Next can be null, bc we don't know items after that,
                                                                                     //Prev is always not null, bc we know items before that.

                        current.PrevItem = current.NextItem = null;
                        if (current is IDisposable disposable)
                            disposable.Dispose();
                        else if (current is IAsyncDisposable adisposable)
                            adisposable.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();

                        previous.NextItem = current = next;
                        if (current is null)
                            return removed;
                        else
                            current.PrevItem = previous;
                    }
                    else
                        current = current.NextItem;
                }
                while (current is not null);

                return removed;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> ClearEmptyAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                Item? current;
                if (firstItem.NextItem is null) //We must never check first item for empty
                    return 0;
                else
                    current = firstItem.NextItem;
                int icap = ItemCapacity, removed = 0;

                do
                {
                    if (current.Count <= 0)
                    {
                        removed += icap;
                        IncrementVersion();
                        IncrementCapacity(icap);
                        Item? next = current.NextItem, previous = current.PrevItem!; //Next can be null, bc we don't know items after that,
                                                                                     //Prev is always not null, bc we know items before that.

                        current.PrevItem = current.NextItem = null;
                        if (current is IDisposable disposable)
                            disposable.Dispose();
                        else if (current is IAsyncDisposable adisposable)
                            await adisposable.DisposeAsync();

                        previous.NextItem = current = next;
                        if (current is null)
                            return removed;
                        else
                            current.PrevItem = previous;
                    }
                    else
                        current = current.NextItem;
                }
                while (current is not null);

                return removed;
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Copy

        /// <inheritdoc/>
        public virtual int CopyTo(TModel[] array, int index = 0, int count = -1, CancellationToken token = default)
        {
            throw new NotImplementedException();
            //if (count == 0)
            //    return 0;
            //ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            //ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, array.Length, nameof(index));
            //if (count < 0)
            //{
            //    count = array.Length - index;
            //    if (count <= 0)
            //        throw new ArgumentOutOfRangeException(nameof(index), "index must be less than array.Length");
            //}
            //else if ((array.Length - index) < count)
            //    throw new ArgumentOutOfRangeException(nameof(count), "count must be less than array.Length - index");

            //token.ThrowIfCancellationRequested();
            //semaphore.Wait(token);
            //try
            //{
            //    Item current = firstItem;
            //    int icap = ItemCapacity;
                
            //}
            //finally
            //{
            //    semaphore.Release();
            //}
        }

        /// <inheritdoc/>
        public virtual async Task<int> CopyToAsync(TModel[] array, int index = 0, int count = -1, CancellationToken token = default)
            => throw new NotImplementedException();

        #endregion

        #region Enumeration

        /// <inheritdoc/>
        public virtual async IAsyncEnumerator<TModel> GetAsyncEnumerator(CancellationToken token = default)
        {
            if (count == 0)
                yield break;
            token.ThrowIfCancellationRequested();
            Item? current = firstItem;
            int icap = ItemCapacity;
            long iterationVersion = Interlocked.Read(in version);
            TModel[] cache = ArrayPool<TModel>.Shared.Rent(enumerationCacheSz);

            try
            {
                int cachePtr = -1, itemStackPtr = 0, cachedCount = 0;

                do
                {
                    if (cachedCount != 0) //We have a cache
                    {
                        yield return cache[cachePtr];
                        if (++cachePtr == cache.Length || cachePtr == cachedCount)
                        {
                            cachePtr = -1;
                            cachedCount = 0;
                        }
                    }
                    else //We don't have a cache, lets full it
                    {
                        await semaphore.WaitAsync(token);
                        try
                        {
                            long ver = Interlocked.Read(in version);
                            if (iterationVersion != ver)
                                throw new InvalidDataException($"PoolActiveStack version mismatch. Iteration version: {iterationVersion}; Data version: {ver}");

                            for (cachePtr = cachedCount = 0;
                                cachePtr < cache.Length;
                                itemStackPtr++, cachePtr++, cachedCount++)
                            {
                                if (itemStackPtr == icap)
                                {
                                    current = current!.NextItem;
                                    if (current is null)
                                        goto ExitFor;
                                    else
                                    {
                                        itemStackPtr = -1;
                                        cachePtr--;
                                        cachedCount--;
                                    }
                                }
                                else
                                {
                                    while (current!.Stack[itemStackPtr].Refs <= 0)
                                        if (itemStackPtr == icap)
                                        {
                                            current = current.NextItem;
                                            if (current is null)
                                                goto ExitFor;
                                            else
                                                itemStackPtr = 0;
                                        }
                                        else
                                            itemStackPtr++;

                                    cache[cachePtr] = current.Stack[itemStackPtr].Model!;
                                }
                            }
                        ExitFor:;

                            if (cachedCount == 0)
                                yield break; //Exit enumerator, its an end.
                            else
                                cachePtr = 0;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                    token.ThrowIfCancellationRequested();
                }
                while (cachePtr != -1 || current is not null);
            }
            finally
            {
                ArrayPool<TModel>.Shared.Return(cache, true);
            }
        }

        public virtual IEnumerator<TModel> GetEnumerator(CancellationToken token)
        {
            if (count == 0)
                yield break;
            token.ThrowIfCancellationRequested();
            Item? current = firstItem;
            int icap = ItemCapacity;
            long iterationVersion = Interlocked.Read(in version);
            TModel[] cache = ArrayPool<TModel>.Shared.Rent(enumerationCacheSz);

            try
            {
                int cachePtr = -1, itemStackPtr = 0, cachedCount = 0;

                do
                {
                    if (cachedCount != 0) //We have a cache
                    {
                        yield return cache[cachePtr];
                        if (++cachePtr == cache.Length || cachePtr == cachedCount)
                        {
                            cachePtr = -1;
                            cachedCount = 0;
                        }
                    }
                    else //We don't have a cache, lets full it
                    {
                        semaphore.Wait(token);
                        try
                        {
                            long ver = Interlocked.Read(in version);
                            if (iterationVersion != ver)
                                throw new InvalidDataException($"PoolActiveStack version mismatch. Iteration version: {iterationVersion}; Data version: {ver}");

                            for (cachePtr = cachedCount = 0;
                                cachePtr < cache.Length;
                                itemStackPtr++, cachePtr++, cachedCount++)
                            {
                                if (itemStackPtr == icap)
                                {
                                    current = current!.NextItem;
                                    if (current is null)
                                        goto ExitFor;
                                    else
                                    {
                                        itemStackPtr = -1;
                                        cachePtr--;
                                        cachedCount--;
                                    }
                                }
                                else
                                {
                                    while (current!.Stack[itemStackPtr].Refs <= 0)
                                        if (itemStackPtr == icap)
                                        {
                                            current = current.NextItem;
                                            if (current is null)
                                                goto ExitFor;
                                            else
                                                itemStackPtr = 0;
                                        }
                                        else
                                            itemStackPtr++;

                                    cache[cachePtr] = current.Stack[itemStackPtr].Model!;
                                }
                            }
                        ExitFor:;

                            if (cachedCount == 0)
                                yield break; //Exit enumerator, its an end.
                            else
                                cachePtr = 0;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                    token.ThrowIfCancellationRequested();
                }
                while (cachePtr != -1 || current is not null);
            }
            finally
            {
                ArrayPool<TModel>.Shared.Return(cache, true);
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerator<TModel> GetEnumerator() => GetEnumerator(default); //Call tokenized method to decrease output file sz and code length

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Defragmentation

        protected virtual int DefragmentationCore(CancellationToken token = default)
        {
            Item? freeItem = firstItem, currentItem;
            int icap = ItemCapacity, freePtr = 0, notFreePtr, defragmentedCount = 0;

            if (FindFreeIndex())
            {
                if (freeItem.NextItem is not null) //Check next on null, bc we need next items to move items from it to left
                {
                    currentItem = freeItem.NextItem;
                    while (currentItem.NextItem is not null)
                        currentItem = currentItem.NextItem; //Move to the rightest item (For performance)

                    notFreePtr = icap - 1;
                    do
                    {
                        if (notFreePtr < 0)
                        {
                            notFreePtr = icap - 1;
                            currentItem = currentItem.PrevItem;
                            if (currentItem is null || currentItem == freeItem)
                                break;
                        }
                        else if (currentItem.Stack[notFreePtr].Refs > 0)
                        {
                            freeItem.Stack[freePtr] = currentItem.Stack[notFreePtr];

                            freeItem.IncrementCount();
                            currentItem.Stack[notFreePtr] = (0, default);
                            currentItem.DecrementCount();

                            defragmentedCount++;
                        }
                        notFreePtr--;
                    }
                    while (FindFreeIndex());
                }

                /*//That for should be not in else block bc. after moving from other item we can have free space here, but with 'holes'
                for (notFreePtr = icap - 1; freePtr < icap && notFreePtr != freePtr;)
                {
                    if (freeItem.Stack[freePtr].Refs <= 0) //It's a free space
                    {
                        while (freeItem.Stack[notFreePtr].Refs <= 0)
                            if (--notFreePtr == freePtr)
                            {
                                if (defragmentedCount != 0)
                                    IncrementVersion();
                                return defragmentedCount; //Exit 'early' bc there is nothing to move from right to left
                            }

                        freeItem.Stack[freePtr] = freeItem.Stack[notFreePtr];
                        freeItem.Stack[notFreePtr] = (0, default); //'Swap' (its more move than swap)

                        freePtr++;
                        notFreePtr--; //Change ptrs
                    }
                    else
                        freePtr++;
                }*/

                if (defragmentedCount != 0)
                    IncrementVersion();
                return defragmentedCount;
            }
            else
                return 0;

            bool FindFreeIndex()
            {
                //At first we must find item, which have a free space
                while (freeItem.FreeCount <= 0)
                {
                    freeItem = freeItem.NextItem;
                    freePtr = 0;
                    if (freeItem is null)
                        return false; //We don't find free space
                }
                for (; freePtr < icap; freePtr++)
                    if (freeItem.Stack[freePtr].Refs <= 0) //We found free space at ptr
                        return true;
                return false; //that return will never work if our code is OK. Bc FreeCount <= 0 only when we have in stack model with <= 0 refs
            }
        }

        /// <summary>
        /// Moves all items from right to left, to use empty space between other items. That method doesn't clear empty blocks
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Count of defragmented space</returns>
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

        /// <summary>
        /// Moves all items from right to left async, to use empty space between other items. That method doesn't clear empty blocks
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Count of defragmented space</returns>
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

            protected volatile int count = 0;
            /// <summary>
            /// Count of <typeparamref name="TModel"/>s in <see cref="Stack"/>
            /// </summary>
            public virtual int Count
            {
                get => count;
                set => count = value;
            }
            public virtual void IncrementCount() => Interlocked.Increment(ref count);
            public virtual void DecrementCount() => Interlocked.Decrement(ref count);

            /// <summary>
            /// Calculates count of free space in <see cref="Stack"/>
            /// </summary>
            public virtual int FreeCount => Stack.Length - Count;

            protected volatile Item? prev, next;
            /// <summary>
            /// Next <see cref="Item"/> or null
            /// </summary>
            public virtual Item? NextItem
            {
                get => next;
                set => next = value;
            }
            /// <summary>
            /// Previous <see cref="Item"/> or null
            /// </summary>
            public virtual Item? PrevItem
            {
                get => prev;
                set => prev = value;
            }
        }

        #endregion
    }
}
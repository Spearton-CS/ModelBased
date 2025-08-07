using System.Collections;

namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;
    using System.Runtime.CompilerServices;

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

        protected virtual int AddCore(TModel model, int refs, CancellationToken token = default)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            ((int, TModel?)[] Stack, int Index)? empty = null;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                {
                    TModel? stackModel = current.Stack[i].Model;
                    if (stackModel is not null)
                    {
                        if (stackModel.EqualsByID(model.ID))
                        {
                            IncrementVersion();
                            return (current.Stack[i].Refs += refs); //We found 
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
        public IEnumerable<int> AddMany(IEnumerable<TModel> models, int refs = 1, CancellationToken token = default)
        {
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
        public async IAsyncEnumerable<int> AddManyAsync(IEnumerable<TModel> models, int refs = 1, [EnumeratorCancellation] CancellationToken token = default)
        {
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
        public async IAsyncEnumerable<int> AddManyAsync(IAsyncEnumerable<TModel> models, int refs = 1, [EnumeratorCancellation] CancellationToken token = default)
        {
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
        public void AddManyIgnore(IEnumerable<TModel> models, int refs = 1, CancellationToken token = default)
        {
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
        public async Task AddManyIgnoreAsync(IEnumerable<TModel> models, int refs = 1, CancellationToken token = default)
        {
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
        public async Task AddManyIgnoreAsync(IAsyncEnumerable<TModel> models, int refs = 1, CancellationToken token = default)
        {
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

        /// <inheritdoc/>
        public virtual (int Refs, TModel? Model) TryRef(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual int TryRef(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<(int Refs, TModel? Model)> TryRefAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<int> TryRefAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<(int Refs, TModel? Model)> TryRefMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> TryRefMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<(int Refs, TModel? Model)> TryRefManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<int> TryRefManyAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<(int Refs, TModel? Model)> TryRefManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<int> TryRefManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void TryRefManyIgnore(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void TryRefManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task TryRefManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task TryRefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task TryRefManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task TryRefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Unref

        /// <inheritdoc/>
        public virtual (int Refs, TModel? Model) TryUnref(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual int TryUnref(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<(int Refs, TModel? Model)> TryUnrefAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<int> TryUnrefAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<(int Refs, TModel? Model)> TryUnrefMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> TryUnrefMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<(int Refs, TModel? Model)> TryUnrefManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<int> TryUnrefManyAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<(int Refs, TModel? Model)> TryUnrefManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<int> TryUnrefManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void TryUnrefIgnoreMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void TryUnrefIgnoreMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task TryUnrefManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task TryUnrefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task TryUnrefManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task TryUnrefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Unref or remove

        /// <inheritdoc/>
        public virtual (int Refs, TModel? Model) UnrefOrRemove(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual int UnrefOrRemove(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<(int Refs, TModel? Model)> UnrefOrRemoveAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<int> UnrefOrRemoveAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<(int Refs, TModel? Model)> UnrefOrRemoveMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> UnrefOrRemoveMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<(int Refs, TModel? Model)> UnrefOrRemoveManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<int> UnrefOrRemoveManyAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<(int Refs, TModel? Model)> UnrefOrRemoveManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<int> UnrefOrRemoveManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void UnrefOrRemoveIgnoreMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void UnrefOrRemoveIgnoreMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task UnrefOrRemoveManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task UnrefOrRemoveManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task UnrefOrRemoveManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task UnrefOrRemoveManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Searching

        /// <inheritdoc/>
        public virtual bool Contains(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<bool> ContainsAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Clear

        /// <inheritdoc/>
        public virtual int ClearEmpty(CancellationToken token = default)
        {
            throw new NotImplementedException();
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
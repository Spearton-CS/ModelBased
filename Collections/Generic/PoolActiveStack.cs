using System.Collections;

namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;

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

        #region Add/remove

        /// <summary>
        /// Used by <see cref="Add(TModel, int, CancellationToken)"/> and <see cref="AddAsync(TModel, int, CancellationToken)"/>
        /// after locking through <see cref="semaphore"/>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="refs"></param>
        protected virtual int AddCore(TModel model, int refs)
        {
            Item current = firstItem;
            int icap = ItemCapacity;
            ((int, TModel)[] Stack, int Index)? empty = null;
            while (true)
            {
                for (int i = 0; i < icap; i++)
                    if (current.Stack[i].Model is not null)
                    {
                        if (model.EqualsByID(current.Stack[i].Model!.ID)) //Is model
                        {
                            current.Stack[i].Refs += refs;
                            return current.Stack[i].Refs;
                        }
                        //Is not model
                    }
                    else if (empty is null)
                    {

                        current.Stack[i] = (refs, model);
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
                return AddCore(model, refs);
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
                return AddCore(model, refs);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual (bool Success, TModel? Model) Remove(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual bool Remove(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<(bool Success, TModel? Model)> RemoveAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<bool> RemoveAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Ref/unref

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

        #endregion

        #region Unref/remove

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

        #endregion

        #region Clear

        public virtual int ClearEmpty(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

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
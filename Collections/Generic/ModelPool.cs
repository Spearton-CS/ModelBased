using System.Collections;

namespace ModelBased.Collections.Generic
{
    using ComponentModel;

    public class ModelPool<TModel, TID> : IModelPool<ModelPool<TModel, TID>, TModel, TID>
        where TID : notnull
        where TModel : IDataModel<TModel, TID>
    {
        protected IPoolActiveStack<TModel, TID> activeStack = [];
        protected IPoolShadowStack<TModel, TID> shadowStack;
        protected SemaphoreSlim semaphore = new(1, 1);
        
        protected ModelPool() { }
        public ModelPool(int shadowStackCapacity = 0)
        {
            shadowStack = new PoolShadowStack<TModel, TID>(shadowStackCapacity);
        }

        #region Properties

        public static ModelPool<TModel, TID> Shared => throw new NotImplementedException();

        public virtual int ShadowCapacity => throw new NotImplementedException();

        public virtual int ShadowCount => throw new NotImplementedException();

        public virtual int Count => throw new NotImplementedException();

        #endregion

        #region Clear shadow

        public virtual int ClearShadow(int minOld = -1, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<int> ClearShadowAsync(int minOld = -1, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Searching

        public virtual bool Contains(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> ContainsAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsRented(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> IsRentedAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<bool> IsRentedMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> IsRentedManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> IsRentedManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Modify

        public virtual bool Modify(TID id, TModel mod, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool Modify(TModel src, TModel mod, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> ModifyAsync(TID id, TModel mod, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> ModifyAsync(TModel src, TModel mod, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<bool> ModifyMany(IEnumerable<(TID, TModel)> idWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<bool> ModifyMany(IEnumerable<(TModel, TModel)> srcWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ModifyManyAsync(IEnumerable<(TID, TModel)> idWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ModifyManyAsync(IAsyncEnumerable<(TID, TModel)> idWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ModifyManyAsync(IEnumerable<(TModel, TModel)> srcWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ModifyManyAsync(IAsyncEnumerable<(TModel, TModel)> srcWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<bool> ModifyManyIgnore(IEnumerable<(TID, TModel)> idWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool ModifyManyIgnore(IEnumerable<(TModel, TModel)> srcWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ModifyManyIgnoreAsync(IEnumerable<(TID, TModel)> idWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ModifyManyIgnoreAsync(IAsyncEnumerable<(TID, TModel)> idWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> ModifyManyIgnoreAsync(IEnumerable<(TModel, TModel)> srcWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> ModifyManyIgnoreAsync(IAsyncEnumerable<(TModel, TModel)> srcWithMods, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Rent/Return

        public virtual TModel Rent(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<TModel> RentAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<TModel> RentMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<TModel> RentManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<TModel> RentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool Return(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> ReturnAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<bool> ReturnMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ReturnManyAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<bool> ReturnManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool ReturnManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> ReturnManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> ReturnManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool TryRent(TID id, out TModel? model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<(bool Success, TModel? Result)> TryRentAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<(bool Success, TModel? Result)> TryRentMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<(bool Success, TModel? Result)> TryRentManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<(bool Success, TModel? Result)> TryRentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Subscribe/Desubscribe

        public virtual int Subscribe(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual int Subscribe(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<int> SubscribeAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<int> SubscribeAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<int> SubscribeMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<int> SubscribeMany(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool SubscribeManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool SubscribeManyIgnore(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> SubscribeManyIgnoreAsync(IEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> SubscribeManyIgnoreAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> SubscribeManyIgnoreAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> SubscribeManyIgnoreAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual int Desubscribe(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual int Desubscribe(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<int> DesubscribeAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<int> DesubscribeAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<int> DesubscribeMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<int> DesubscribeMany(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool DesubscribeManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual bool DesubscribeManyIgnore(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> DesubscribeManyIgnoreAsync(IEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> DesubscribeManyIgnoreAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> DesubscribeManyIgnoreAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> DesubscribeManyIgnoreAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Enumeration

        public virtual IEnumerator<TID> EnumerateIDs(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerator<TID> EnumerateIDsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual async IAsyncEnumerator<TModel> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerator<TID> GetEnumerator(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerator<TModel> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
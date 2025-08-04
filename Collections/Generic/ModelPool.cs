namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;

    public class ModelPool<TModel, TID> : IModelPool<ModelPool<TModel, TID>, TModel, TID>
        where TID : notnull
        where TModel : IDataModel<TModel, TID>
    {
        protected LinkedList<(TModel Model, int Refs)> models = [];
        protected IPoolShadowStack<TModel, TID> shadowModels;
        protected SemaphoreSlim semaphore = new(1, 1);
        
        public ModelPool(int shadowStackCapacity = 0)
        {
            
        }

        public static ModelPool<TModel, TID> Shared => throw new NotImplementedException();

        public int ShadowCapacity => throw new NotImplementedException();

        public int ShadowCount => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public int ClearShadow(int minOld = -1)
        {
            throw new NotImplementedException();
        }

        public Task<int> ClearShadowAsync(int minOld = -1, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool Contains(TID id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ContainsAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool IsRented(TID id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsRentedAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<bool> IsRentedMany(IEnumerable<TID> ids)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<bool> IsRentedManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<bool> IsRentedManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public TModel Rent(TID id)
        {
            throw new NotImplementedException();
        }

        public Task<TModel> RentAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TModel> RentMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TModel> RentManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TModel> RentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool Return(TModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReturnAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<bool> ReturnMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<bool> ReturnManyAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<bool> ReturnManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Now we dont need CopyTo - its not implemented")]
        public int ToArray(TModel[] array, int index = 0, int count = -1)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Now we dont need CopyTo - its not implemented")]
        public Task<int> ToArrayAsync(TModel[] array, int index = 0, int count = -1, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool TryRent(TID id, out TModel? model)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, TModel? Result)> TryRentAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(bool Success, TModel? Result)> TryRentMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<(bool Success, TModel? Result)> TryRentManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<(bool Success, TModel? Result)> TryRentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #region Enumeration

        public IEnumerable<TID> EnumerateIDs()
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TID> EnumerateIDsAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TModel> AsAsyncEnumerable(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TModel> AsEnumerable()
        {
            throw new NotImplementedException();
        }

        public bool TryRent(TID id, out TModel? model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public TModel Rent(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool Return(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool ReturnManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReturnManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReturnManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public int ClearShadow(int minOld = -1, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool IsRented(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<bool> IsRentedMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool Contains(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public int ToArray(TModel[] array, int index = 0, int count = -1, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TID> EnumerateIDsAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TModel> AsAsyncEnumerable()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;

    /// <summary>
    /// Pool of <typeparamref name="TModel"/>, which can be rented, returned and subscribed by <typeparamref name="TID"/>
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TID"></typeparam>
    public interface IModelPool<TModel, TID>
        where TID : notnull
        where TModel : notnull, IDataModelContract<TModel, TID> //Exactly TModel, TID, bc we need Factory
    {
        #region Rent/Return

        /// <summary>
        /// Tries to rent <typeparamref name="TModel"/> with <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns>True, if <paramref name="model"/> is not null (default)</returns>
        bool TryRent(TID id, out TModel? model);
        /// <summary>
        /// Tries to rent <typeparamref name="TModel"/> async with <paramref name="id"/>. Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Success, if Result is not null (default)</returns>
        Task<(bool Success, TModel? Result)> TryRentAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Tries to rent many <typeparamref name="TModel"/> by one transaction. Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Success, if Result is not null (default)</returns>
        IEnumerable<(bool Success, TModel? Result)> TryRentMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to rent many <typeparamref name="TModel"/> async by one transaction. Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Success, if Result is not null (default)</returns>
        IAsyncEnumerable<(bool Success, TModel? Result)> TryRentManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to rent many <typeparamref name="TModel"/> async by one transaction. Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Success, if Result is not null (default)</returns>
        IAsyncEnumerable<(bool Success, TModel? Result)> TryRentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// Rents <typeparamref name="TModel"/> with <paramref name="id"/>. If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModelContract{TSelf, TID}.Factory(TID)"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TModel Rent(TID id);
        /// <summary>
        /// Rents <typeparamref name="TModel"/> with <paramref name="id"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModelContract{TSelf, TID}.Factory(TID)"/>.
        /// It can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<TModel> RentAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/>.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModelContract{TSelf, TID}.Factory(TID)"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IEnumerable<TModel> RentMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModelContract{TSelf, TID}.Factory(TID)"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<TModel> RentManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModelContract{TSelf, TID}.Factory(TID)"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<TModel> RentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// Returns <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns>True, if returned</returns>
        bool Return(TModel model);
        /// <summary>
        /// Returns <paramref name="model"/> async. Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>True, if returned</returns>
        Task<bool> ReturnAsync(TModel model, CancellationToken token = default);

        /// <summary>
        /// Returns <paramref name="models"/>. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if returned</returns>
        IEnumerable<bool> ReturnMany(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Returns <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if returned</returns>
        IAsyncEnumerable<bool> ReturnManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Returns <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if returned</returns>
        IAsyncEnumerable<bool> ReturnManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        #endregion

        //#region Subscribe/Desubscribe

        //int Subscribe(TModel model);
        //int Subscribe(TID id);

        //Task<int> SubscribeAsync(TID id, CancellationToken token = default);
        //Task<int> SubscribeAsync(TModel model, CancellationToken token = default);

        //int SubscribeMany(IEnumerable<TModel> models);
        //int SubscribeMany(IEnumerable<TID> id);

        //IAsyncEnumerable<int> SubscribeManyAsync(IEnumerable<TModel> model, CancellationToken token = default);
        //IAsyncEnumerable<int> SubscribeManyAsync(IEnumerable<TID> id, CancellationToken token = default);
        //IAsyncEnumerable<int> SubscribeManyAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default);
        //IAsyncEnumerable<int> SubscribeManyAsync(IAsyncEnumerable<TID> id, CancellationToken token = default);

        //int Desubscribe(TModel model);
        //int Desubscribe(TID id);

        //Task<int> DesubscribeAsync(TModel model, CancellationToken token = default);
        //Task<int> DesubscribeAsync(TID id, CancellationToken token = default);

        //int DesubscribeMany(IEnumerable<TModel> models);
        //int DesubscribeMany(IEnumerable<TID> id);

        //IAsyncEnumerable<int> DesubscribeManyAsync(IEnumerable<TModel> model, CancellationToken token = default);
        //IAsyncEnumerable<int> DesubscribeManyAsync(IEnumerable<TID> id, CancellationToken token = default);
        //IAsyncEnumerable<int> DesubscribeManyAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default);
        //IAsyncEnumerable<int> DesubscribeManyAsync(IAsyncEnumerable<TID> id, CancellationToken token = default);

        //#endregion

        #region Shadow stack

        /// <summary>
        /// Clears <see cref="IPoolShadowStack{TModel, TID}"/>
        /// </summary>
        /// <param name="minOld">Minimal old of shadow model (its not time, its count of modification of shadow stack after that model)</param>
        /// <returns>Count of cleaned shadow models (0-ref)</returns>
        int ClearShadow(int minOld = -1);
        /// <summary>
        /// Clears <see cref="IPoolShadowStack{TModel, TID}"/>. Can be canceled
        /// </summary>
        /// <param name="minOld">Minimal old of shadow model (its not time, its count of modification of shadow stack after that model)</param>
        /// <param name="token"></param>
        /// <returns>Count of cleaned shadow models (0-ref)</returns>
        Task<int> ClearShadowAsync(int minOld = -1, CancellationToken token = default);

        /// <summary>
        /// <see cref="IPoolShadowStack{TModel, TID}.Capacity"/>
        /// </summary>
        int ShadowCapacity { get; }
        /// <summary>
        /// Count of models, which shadowed. (Count by this <see cref="IModelPool{TModel, TID}"/>)
        /// </summary>
        int ShadowCount { get; }

        #endregion

        #region Check

        /// <summary>
        /// Check, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Excluding shadow models
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True, if rented</returns>
        bool IsRented(TID id);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Excluding shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        Task<bool> IsRentedAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Check, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Excluding shadow models
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>True, if rented</returns>
        IEnumerable<bool> IsRentedMany(IEnumerable<TID> ids);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Excluding shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        IAsyncEnumerable<bool> IsRentedManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Excluding shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        IAsyncEnumerable<bool> IsRentedManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// Check, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Including shadow models
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True, if contains</returns>
        bool Contains(TID id);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Including shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        Task<bool> ContainsAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Check, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Including shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Including shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Including shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        #endregion

        #region Properties

        /// <summary>
        /// Count of active models. Excluding shadow models.
        /// </summary>
        int Count { get; }

        #endregion

        #region Copy

        /// <summary>
        /// Copies active models to <paramref name="array"/> into index <paramref name="index"/> up to <paramref name="count"/>.
        /// Excluding shadow models
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index">Start index in <paramref name="array"/></param>
        /// <param name="count"></param>
        /// <returns>Count of copied items</returns>
        int ToArray(TModel[] array, int index = 0, int count = -1);
        /// <summary>
        /// Copies active models to <paramref name="array"/> into index <paramref name="index"/> up to <paramref name="count"/> async.
        /// Excluding shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index">Start index in <paramref name="array"/></param>
        /// <param name="count"></param>
        /// <param name="token"></param>
        /// <returns>Count of copied items</returns>
        Task<int> ToArrayAsync(TModel[] array, int index = 0, int count = -1, CancellationToken token = default);

        #endregion

        #region Enumeration

        /// <summary>
        /// Enumerates <typeparamref name="TID"/>s of active <typeparamref name="TModel"/>s.
        /// Excluding shadow.
        /// Every modification of this <see cref="IModelPool{TModel, TID}"/> will crush this enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerable<TID> EnumerateIDs();
        /// <summary>
        /// Enumerates <typeparamref name="TID"/>s of active <typeparamref name="TModel"/>s async.
        /// Excluding shadow.
        /// Can be canceled.
        /// Every modification of this <see cref="IModelPool{TModel, TID}"/> will crush this enumerator
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<TID> EnumerateIDsAsync(CancellationToken token = default);
        /// <summary>
        /// Enumerates active <typeparamref name="TModel"/>s.
        /// Excluding shadow.
        /// Every modification of this <see cref="IModelPool{TModel, TID}"/> will crush this enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerable<TModel> AsEnumerable();
        /// <summary>
        /// Enumerates active <typeparamref name="TModel"/>s async.
        /// Excluding shadow.
        /// Every modification of this <see cref="IModelPool{TModel, TID}"/> will crush this enumerator
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<TModel> AsAsyncEnumerable(CancellationToken token = default);

        #endregion
    }

    /// <summary>
    /// Pool of <typeparamref name="TModel"/>, which can be rented, returned and subscribed by <typeparamref name="TID"/>.
    /// This one have <see cref="Shared"/> property
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TID"></typeparam>
    public interface IModelPool<TSelf, TModel, TID> : IModelPool<TModel, TID>
        where TID : notnull
        where TModel : notnull, IDataModelContract<TModel, TID>
        where TSelf : notnull, IModelPool<TSelf, TModel, TID>
    {
        /// <summary>
        /// Shared <typeparamref name="TSelf"/>, which can be used by any thread.
        /// </summary>
        static abstract TSelf Shared { get; }
    }
}
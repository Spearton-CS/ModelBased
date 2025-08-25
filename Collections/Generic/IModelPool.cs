namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;

    /// <summary>
    /// Pool of <typeparamref name="TModel"/>, which can be rented, returned and subscribed by <typeparamref name="TID"/>
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TID"></typeparam>
    public interface IModelPool<TModel, TID> : IEnumerable<TModel>, IAsyncEnumerable<TModel>
        where TID : notnull
        where TModel : notnull, IDataModel<TModel, TID> //Exactly TModel, TID, bc we need Factory
    {
        #region Rent/Return

        /// <summary>
        /// Rents <typeparamref name="TModel"/> with <paramref name="id"/>. If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        (TModel Model, int Refs) Rent(TID id, CancellationToken token = default);
        /// <summary>
        /// Rents <typeparamref name="TModel"/> with <paramref name="id"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory"/>.
        /// It can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(TModel Model, int Refs)> RentAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/>.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IEnumerable<(TModel Model, int Refs)> RentMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<(TModel Model, int Refs)> RentManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<(TModel Model, int Refs)> RentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// Returns <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not returned</returns>
        int TryReturn(TModel model, CancellationToken token = default);
        /// <summary>
        /// Returns <paramref name="model"/> async. Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not returned</returns>
        Task<int> TryReturnAsync(TModel model, CancellationToken token = default);

        /// <summary>
        /// Returns <paramref name="models"/>. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not returned</returns>
        IEnumerable<int> TryReturnMany(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Returns <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not returned</returns>
        IAsyncEnumerable<int> TryReturnManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Returns <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not returned</returns>
        IAsyncEnumerable<int> TryReturnManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Returns <paramref name="models"/>. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if returned</returns>
        bool TryReturnManyIgnore(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Returns <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if returned</returns>
        Task<bool> TryReturnManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Returns <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if returned</returns>
        Task<bool> TryReturnManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        #endregion

        #region Modify

        /// <summary>
        /// Tries to modify <paramref name="src"/> using <paramref name="mod"/>. Can be canceled
        /// As default, ID of <paramref name="mod"/> must be equal to <paramref name="src"/>'s ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="src"></param>
        /// <param name="mod"></param>
        /// <param name="token"></param>
        /// <returns>True, if modified</returns>
        public bool TryModify<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        /// <summary>
        /// Tries to modify <paramref name="src"/> using <paramref name="mod"/> async. Can be canceled
        /// As default, ID of <paramref name="mod"/> must be equal to <paramref name="src"/>'s ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="src"></param>
        /// <param name="mod"></param>
        /// <param name="token"></param>
        /// <returns>True, if modified</returns>
        public Task<bool> TryModifyAsync<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        /// <summary>
        /// Tries to modify <paramref name="src"/> using <paramref name="mod"/> async. Can be canceled
        /// As default, ID of <paramref name="mod"/> must be equal to <paramref name="src"/>'s ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="src"></param>
        /// <param name="mod"></param>
        /// <param name="token"></param>
        /// <returns>True, if modified</returns>
        public Task<bool> TryModifyAsyncA<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        /// <summary>
        /// Tries to modify sourceModels using modifyModels. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if modified</returns>
        public IEnumerable<bool> TryModifyMany<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        /// <summary>
        /// Tries to modify sourceModels using modifyModels async. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if modified</returns>
        public IAsyncEnumerable<bool> TryModifyManyAsync<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        /// <summary>
        /// Tries to modify sourceModels using modifyModels async. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if modified</returns>
        public IAsyncEnumerable<bool> TryModifyManyAsync<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        /// <summary>
        /// Tries to modify sourceModels using modifyModels async. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if modified</returns>
        public IAsyncEnumerable<bool> TryModifyManyAsyncA<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;
        /// <summary>
        /// Tries to modify sourceModels using modifyModels async. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if modified</returns>
        public IAsyncEnumerable<bool> TryModifyManyAsyncA<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        /// <summary>
        /// Tries to modify sourceModels using modifyModels. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if all modified</returns>
        public bool TryModifyManyIgnore<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        /// <summary>
        /// Tries to modify sourceModels using modifyModels async. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if all modified</returns>
        public Task<bool> TryModifyManyIgnoreAsync<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        /// <summary>
        /// Tries to modify sourceModels using modifyModels async. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if all modified</returns>
        public Task<bool> TryModifyManyIgnoreAsync<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        /// <summary>
        /// Tries to modify sourceModels using modifyModels async. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if all modified</returns>
        public Task<bool> TryModifyManyIgnoreAsyncA<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;
        /// <summary>
        /// Tries to modify sourceModels using modifyModels async. Can be canceled
        /// As default, ID of modifyModel must be equal to sourceModel's ID
        /// </summary>
        /// <typeparam name="TUpdateableModel"></typeparam>
        /// <param name="srcWithMods"></param>
        /// <param name="token"></param>
        /// <returns>True, if all modified</returns>
        public Task<bool> TryModifyManyIgnoreAsyncA<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        #endregion

        #region Ref/unref

        /// <summary>
        /// Tries to ref <paramref name="model"/>. Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1</returns>
        int TryRef(TModel model, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="model"/> async. Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1</returns>
        Task<int> TryRefAsync(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to ref <paramref name="models"/>. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1</returns>
        IEnumerable<int> TryRefMany(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to ref <paramref name="models"/>. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if all <paramref name="models"/> have a new refs</returns>
        bool TryRefManyIgnore(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to ref <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1</returns>
        IAsyncEnumerable<int> TryRefManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1</returns>
        IAsyncEnumerable<int> TryRefManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to ref <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if all <paramref name="models"/> have a new refs</returns>
        Task<bool> TryRefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="models"/> async. Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if all <paramref name="models"/> have a new refs</returns>
        Task<bool> TryRefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        #endregion

        #region Shadow stack

        /// <summary>
        /// Clears <see cref="IPoolShadowStack{TModel, TID}"/>
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Count of cleaned shadow models (0-ref)</returns>
        int ClearShadow(CancellationToken token = default);
        /// <summary>
        /// Clears <see cref="IPoolShadowStack{TModel, TID}"/>. Can be canceled
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Count of cleaned shadow models (0-ref)</returns>
        Task<int> ClearShadowAsync(CancellationToken token = default);

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
        /// Check, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent"/>).
        /// Excluding shadow models
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        bool IsRented(TID id, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent"/>).
        /// Excluding shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        Task<bool> IsRentedAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Check, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent"/>).
        /// Excluding shadow models
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        IEnumerable<bool> IsRentedMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent"/>).
        /// Excluding shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        IAsyncEnumerable<bool> IsRentedManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent"/>).
        /// Excluding shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        IAsyncEnumerable<bool> IsRentedManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// Check, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent"/>).
        /// Including shadow models
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        bool Contains(TID id, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent"/>).
        /// Including shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        Task<bool> ContainsAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Check, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent"/>).
        /// Including shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent"/>).
        /// Including shadow models.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> have a reference (called <see cref="Rent"/>).
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

        #region Enumeration

        /// <summary>
        /// Enumerates <typeparamref name="TID"/>s of active <typeparamref name="TModel"/>s.
        /// Excluding shadow.
        /// Every modification of this <see cref="IModelPool{TModel, TID}"/> will crush this enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator<TID> EnumerateIDs(CancellationToken token = default);
        /// <summary>
        /// Enumerates <typeparamref name="TID"/>s of active <typeparamref name="TModel"/>s async.
        /// Excluding shadow.
        /// Every modification of this <see cref="IModelPool{TModel, TID}"/> will crush this enumerator
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerator<TID> EnumerateIDsAsync(CancellationToken token = default);

        /// <summary>
        /// Returns an enumerator that iterates through the collection. Can be canceled
        /// </summary>
        /// <param name="token"></param>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<TModel> GetEnumerator(CancellationToken token = default);

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
        where TModel : notnull, IDataModel<TModel, TID>
        where TSelf : notnull, IModelPool<TSelf, TModel, TID>
    {
        /// <summary>
        /// Shared <typeparamref name="TSelf"/>, which can be used by any thread.
        /// </summary>
        static abstract TSelf Shared { get; }
    }
}
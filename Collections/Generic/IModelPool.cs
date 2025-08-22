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
        /// Rents <typeparamref name="TModel"/> with <paramref name="id"/>. If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory(TID)"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        (TModel Model, int Refs) Rent(TID id, CancellationToken token = default);
        /// <summary>
        /// Rents <typeparamref name="TModel"/> with <paramref name="id"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory(TID)"/>.
        /// It can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(TModel Model, int Refs)> RentAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/>.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory(TID)"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IEnumerable<(TModel Model, int Refs)> RentMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory(TID)"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<(TModel Model, int Refs)> RentManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Rents many <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// If its not exist <see cref="IModelPool{TModel, TID}"/> will create it through <see cref="IDataModel{TSelf, TID}.Factory(TID)"/>.
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

        public bool Modify<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public Task<bool> ModifyAsync<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        public Task<bool> ModifyAsyncA<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        public bool Modify<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public Task<bool> ModifyAsync<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        public Task<bool> ModifyAsyncA<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        public IEnumerable<bool> ModifyMany<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        public IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;
        public IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        public IEnumerable<bool> ModifyMany<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        public IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;
        public IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        public bool ModifyManyIgnore<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        public Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;
        public Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        public bool ModifyManyIgnore<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;
        public Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel;

        public Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;
        public Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel;

        #endregion

        #region Ref/unref

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        int TryRef(TModel model, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        int TryRef(TID id, CancellationToken token = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<int> TryRefAsync(TID id, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<int> TryRefAsync(TModel model, CancellationToken token = default);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IEnumerable<int> TryRefMany(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IEnumerable<int> TryRefMany(IEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        bool TryRefManyIgnore(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        bool TryRefManyIgnore(IEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<int> TryRefManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<int> TryRefManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<int> TryRefManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<int> TryRefManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> TryRefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> TryRefManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> TryRefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> TryRefManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="models"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //int TryUnref(TModel models, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //int TryUnref(TID ids, CancellationToken token = default);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="models"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //Task<int> TryUnrefAsync(TModel models, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //Task<int> TryUnrefAsync(TID ids, CancellationToken token = default);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="models"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //IEnumerable<int> TryUnrefMany(IEnumerable<TModel> models, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //IEnumerable<int> TryUnrefMany(IEnumerable<TID> ids, CancellationToken token = default);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="models"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //bool TryUnrefManyIgnore(IEnumerable<TModel> models, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //bool TryUnrefManyIgnore(IEnumerable<TID> ids, CancellationToken token = default);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="models"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //IAsyncEnumerable<int> TryUnrefManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //IAsyncEnumerable<int> TryUnrefManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="models"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //IAsyncEnumerable<int> TryUnrefManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //IAsyncEnumerable<int> TryUnrefManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="models"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //Task<bool> TryUnrefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //Task<bool> TryUnrefManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="models"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //Task<bool> TryUnrefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //Task<bool> TryUnrefManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        #endregion

        #region Shadow stack

        /// <summary>
        /// Clears <see cref="IPoolShadowStack{TModel, TID}"/>
        /// </summary>
        /// <param name="minOld">Minimal old of shadow model (its not time, its count of modification of shadow stack after that model)</param>
        /// <param name="token"></param>
        /// <returns>Count of cleaned shadow models (0-ref)</returns>
        int ClearShadow(CancellationToken token = default);
        /// <summary>
        /// Clears <see cref="IPoolShadowStack{TModel, TID}"/>. Can be canceled
        /// </summary>
        /// <param name="minOld">Minimal old of shadow model (its not time, its count of modification of shadow stack after that model)</param>
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
        /// Check, that <typeparamref name="TModel"/> with <paramref name="id"/> have a reference (called <see cref="Rent(TID)"/>).
        /// Excluding shadow models
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        bool IsRented(TID id, CancellationToken token = default);
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
        /// <param name="token"></param>
        /// <returns>True, if rented</returns>
        IEnumerable<bool> IsRentedMany(IEnumerable<TID> ids, CancellationToken token = default);
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
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        bool Contains(TID id, CancellationToken token = default);
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
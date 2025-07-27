using System.Runtime.CompilerServices;

namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;

    /// <summary>
    /// Pool of active items of <see cref="IModelPool{TModel, TID}"/>
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TID"></typeparam>
    [CollectionBuilder(typeof(PoolActiveStackBuilder), "Create")]
    public interface IPoolActiveStack<TModel, TID> : IEnumerable<TModel>
        where TID : notnull
        where TModel : notnull, IDataModel<TID>
    {
        #region Stats

        /// <summary>
        /// Count of items (includes uninitialized)
        /// </summary>
        int Capacity { get; }
        /// <summary>
        /// Count of initialized items
        /// </summary>
        int Count { get; }

        #endregion

        #region Add/Remove

        /// <summary>
        /// Adds <paramref name="model"/> with <paramref name="refs"/> (default 1), if already exist - adding <paramref name="refs"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="refs">Count of refs to <paramref name="model"/>. Must be more than 0</param>
        /// <param name="token"></param>
        /// <returns>Count of refs</returns>
        int Add(TModel model, int refs = 1, CancellationToken token = default);

        /// <summary>
        /// Adds <paramref name="model"/> with <paramref name="refs"/> (default 1), if already exist - adding <paramref name="refs"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="refs">Count of refs to <paramref name="model"/>. Must be more than 0</param>
        /// <param name="token"></param>
        /// <returns>Count of refs</returns>
        Task<int> AddAsync(TModel model, int refs = 1, CancellationToken token = default);

        /// <summary>
        /// Adds <paramref name="model"/> with <paramref name="refs"/> (default 1), if already exist - adding <paramref name="refs"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="refs">Count of refs to <paramref name="model"/>. Must be more than 0</param>
        /// <param name="token"></param>
        /// <returns>Count of refs</returns>
        IEnumerable<int> AddMany(IEnumerable<TModel> model, int refs = 1, CancellationToken token = default);

        /// <summary>
        /// Adds <paramref name="models"/> with <paramref name="refs"/> (default 1), if already exist - adding <paramref name="refs"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="refs">Count of refs to <paramref name="models"/>. Must be more than 0</param>
        /// <param name="token"></param>
        /// <returns>Count of refs</returns>
        IAsyncEnumerable<int> AddManyAsync(IEnumerable<TModel> models, int refs = 1, CancellationToken token = default);
        /// <summary>
        /// Adds <paramref name="models"/> with <paramref name="refs"/> (default 1), if already exist - adding <paramref name="refs"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="refs">Count of refs to <paramref name="models"/>. Must be more than 0</param>
        /// <param name="token"></param>
        /// <returns>Count of refs</returns>
        IAsyncEnumerable<int> AddManyAsync(IAsyncEnumerable<TModel> models, int refs = 1, CancellationToken token = default);

        /// <summary>
        /// Adds <paramref name="model"/> with <paramref name="refs"/> (default 1), if already exist - adding <paramref name="refs"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="refs">Count of refs to <paramref name="model"/>. Must be more than 0</param>
        /// <param name="token"></param>
        void AddManyIgnore(IEnumerable<TModel> model, int refs = 1, CancellationToken token = default);

        /// <summary>
        /// Adds <paramref name="models"/> with <paramref name="refs"/> (default 1), if already exist - adding <paramref name="refs"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="refs">Count of refs to <paramref name="models"/>. Must be more than 0</param>
        /// <param name="token"></param>
        Task AddManyIgnoreAsync(IEnumerable<TModel> models, int refs = 1, CancellationToken token = default);
        /// <summary>
        /// Adds <paramref name="models"/> with <paramref name="refs"/> (default 1), if already exist - adding <paramref name="refs"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="refs">Count of refs to <paramref name="models"/>. Must be more than 0</param>
        /// <param name="token"></param>
        Task AddManyIgnoreAsync(IAsyncEnumerable<TModel> models, int refs = 1, CancellationToken token = default);


        /// <summary>
        /// Tries to remove model with <paramref name="id"/> from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed. + Model if was exist, or default (null)</returns>
        (bool Success, TModel? Model) Remove(TID id, CancellationToken token = default);
        /// <summary>
        /// Tries to remove <paramref name="model"/> from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed</returns>
        bool Remove(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to remove model with <paramref name="id"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed. + Model if was exist, or default (null)</returns>
        Task<(bool Success, TModel? Model)> RemoveAsync(TID id, CancellationToken token = default);
        /// <summary>
        /// Tries to remove <paramref name="model"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed</returns>
        Task<bool> RemoveAsync(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to remove models with <paramref name="ids"/> from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed. + Model if was exist, or default (null)</returns>
        IEnumerable<(bool Success, TModel? Model)> RemoveMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to remove <paramref name="models"/> from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed</returns>
        IEnumerable<bool> RemoveMany(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to remove models with <paramref name="ids"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed. + Model if was exist, or default (null)</returns>
        IAsyncEnumerable<(bool Success, TModel? Model)> RemoveManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to remove <paramref name="models"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed</returns>
        IAsyncEnumerable<bool> RemoveManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Tries to remove models with <paramref name="ids"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed. + Model if was exist, or default (null)</returns>
        IAsyncEnumerable<(bool Success, TModel? Model)> RemoveManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to remove <paramref name="models"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>True, if removed</returns>
        IAsyncEnumerable<bool> RemoveManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to remove models with <paramref name="ids"/> from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        void RemoveManyIgnore(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to remove <paramref name="models"/> from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        void RemoveManyIgnore(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to remove models with <paramref name="ids"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        Task RemoveManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to remove <paramref name="models"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        Task RemoveManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Tries to remove models with <paramref name="ids"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        Task RemoveManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to remove <paramref name="models"/> from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Removes with any refs count.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        Task RemoveManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        #endregion

        #region Ref/unref

        /// <summary>
        /// Tries to ref model with <paramref name="id"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs + Model or -1 and default (null) if not exist</returns>
        (int Refs, TModel? Model) TryRef(TID id, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="model"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        int TryRef(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to ref model with <paramref name="id"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs + Model or -1 and default (null) if not exist</returns>
        Task<(int Refs, TModel? Model)> TryRefAsync(TID id, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="model"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        Task<int> TryRefAsync(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to ref models with <paramref name="ids"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs + Model or -1 and default (null) if not exist</returns>
        IEnumerable<(int Refs, TModel? Model)> TryRefMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="models"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IEnumerable<int> TryRefMany(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to ref models with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs + Model or -1 and default (null) if not exist</returns>
        IAsyncEnumerable<(int Refs, TModel? Model)> TryRefManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="models"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IAsyncEnumerable<int> TryRefManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Tries to ref models with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs + Model or -1 and default (null) if not exist</returns>
        IAsyncEnumerable<(int Refs, TModel? Model)> TryRefManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="models"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IAsyncEnumerable<int> TryRefManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to ref models with <paramref name="ids"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        void TryRefManyIgnore(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="models"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        void TryRefManyIgnore(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to ref models with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        Task TryRefManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="models"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        Task TryRefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Tries to ref models with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        Task TryRefManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to ref <paramref name="models"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        Task TryRefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref model with <paramref name="id"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        (int Refs, TModel? Model) TryUnref(TID id, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="model"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        int TryUnref(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to unref model with <paramref name="id"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        Task<(int Refs, TModel? Model)> TryUnrefAsync(TID id, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="model"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        Task<int> TryUnrefAsync(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IEnumerable<(int Refs, TModel? Model)> TryUnrefMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IEnumerable<int> TryUnrefMany(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IAsyncEnumerable<(int Refs, TModel? Model)> TryUnrefManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IAsyncEnumerable<int> TryUnrefManyAsync(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IAsyncEnumerable<(int Refs, TModel? Model)> TryUnrefManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or -1 if not exist</returns>
        IAsyncEnumerable<int> TryUnrefManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        void TryUnrefIgnoreMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        void TryUnrefIgnoreMany(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        Task TryUnrefManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        Task TryUnrefManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        Task TryUnrefManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        Task TryUnrefManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        #endregion

        #region Unref/remove

        /// <summary>
        /// Tries to unref model with <paramref name="id"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>
        /// (Count of refs or 0 if remove) + Model
        /// or -1 if not exist + default (null)
        /// </returns>
        (int Refs, TModel? Model) UnrefOrRemove(TID id, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="model"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or 0 if remove or -1 if not exist</returns>
        int UnrefOrRemove(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to unref model with <paramref name="id"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>
        /// (Count of refs or 0 if remove) + Model
        /// or -1 if not exist + default (null)
        /// </returns>
        Task<(int Refs, TModel? Model)> UnrefOrRemoveAsync(TID id, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="model"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or 0 if remove or -1 if not exist</returns>
        Task<int> UnrefOrRemoveAsync(TModel model, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>
        /// (Count of refs or 0 if remove) + Model
        /// or -1 if not exist + default (null)
        /// </returns>
        IEnumerable<(int Refs, TModel? Model)> UnrefOrRemoveMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or 0 if remove or -1 if not exist</returns>
        IEnumerable<int> UnrefOrRemoveMany(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>
        /// (Count of refs or 0 if remove) + Model
        /// or -1 if not exist + default (null)
        /// </returns>
        IAsyncEnumerable<(int Refs, TModel? Model)> UnrefOrRemoveManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or 0 if remove or -1 if not exist</returns>
        IAsyncEnumerable<int> UnrefOrRemoveManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Tries to unref models with <paramref name="ids"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>
        /// (Count of refs or 0 if remove) + Model
        /// or -1 if not exist + default (null)
        /// </returns>
        IAsyncEnumerable<(int Refs, TModel? Model)> UnrefOrRemoveManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        /// <returns>Count of refs or 0 if remove or -1 if not exist</returns>
        IAsyncEnumerable<int> UnrefOrRemoveManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        void UnrefOrRemoveIgnoreMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        void UnrefOrRemoveIgnoreMany(IEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Tries to unref models with <paramref name="ids"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        Task UnrefOrRemoveManyIgnoreAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        Task UnrefOrRemoveManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Tries to unref models with <paramref name="ids"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        Task UnrefOrRemoveManyIgnoreAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Tries to unref <paramref name="models"/>,
        /// if refs equal or less than 0 - removes that from this <see cref="IPoolActiveStack{TModel, TID}"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="models"></param>
        /// <param name="token"></param>
        Task UnrefOrRemoveManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        #endregion

        #region Searching



        #endregion

        #region Clear

        /// <summary>
        /// Clears empty blocks.
        /// Blocks called empty when all items isn't initialized.
        /// Can be canceled
        /// </summary>
        /// <returns></returns>
        int ClearEmpty(CancellationToken token = default);

        /// <summary>
        /// Clears empty blocks async.
        /// Blocks called empty when all items isn't initialized.
        /// Can be canceled
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<int> ClearEmptyAsync(CancellationToken token = default);

        #endregion

        #region Copy

        [Obsolete("Now we dont need CopyTo - its not implemented")]
        int CopyTo(TModel[] array, int index = 0, int count = -1, CancellationToken token = default);
        [Obsolete("Now we dont need CopyTo - its not implemented")]
        Task<int> CopyToAsync(TModel[] array, int index = 0, int count = -1, CancellationToken token = default);

        #endregion
    }

    public static class PoolActiveStackBuilder
    {
        public static IPoolActiveStack<TModel, TID> Create<TModel, TID>()
            where TID : notnull
            where TModel : notnull, IDataModel<TID>
        {
            return new PoolActiveStack<TModel, TID>();
        }
        public static IPoolActiveStack<TModel, TID> Create<TModel, TID>(ReadOnlySpan<TModel> models)
                    where TID : notnull
                    where TModel : notnull, IDataModel<TID>
        {
            PoolActiveStack<TModel, TID> collection = [];
            foreach (TModel model in models)
                _ = collection.Add(model); //We must use Add bc iterators cant use ref structs (ReadOnlySpan) before .NET 9+

            return collection;
        }
    }
}
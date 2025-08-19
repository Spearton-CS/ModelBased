using System.Runtime.CompilerServices;

namespace ModelBased.Collections.Generic
{
    using ModelBased.ComponentModel;

    /// <summary>
    /// Custom collection, called as 'ShadowStack', used by <see cref="ModelPool{TModel, TID}"/> as default.
    /// Its used for a stack of fixed-size elements <typeparamref name="TModel"/>, where when overflowing, the 'old' elements are deleted and replaced with new ones.
    /// Its possible to "return" items from this stack.
    /// This one contains <see cref="IPoolShadowStack{TSelf, TModel, TID}.Empty"/> property, with shared instance
    /// </summary>
    public interface IPoolShadowStack<TSelf, TModel, TID> : IPoolShadowStack<TModel, TID>
        where TID : notnull
        where TModel : notnull, IDataModel<TID>
        where TSelf : notnull, IPoolShadowStack<TSelf, TModel, TID>
    {
        /// <summary>
        /// Shared empty <typeparamref name="TSelf"/>. Recommended to use this instead of paramless constructor
        /// </summary>
        static abstract TSelf Empty { get; }
    }

    /// <summary>
    /// Custom collection, called as 'ShadowStack', used by <see cref="ModelPool{TModel, TID}"/> as default.
    /// Its used for a stack of fixed-size elements <typeparamref name="TModel"/>, where when overflowing, the 'old' elements are deleted and replaced with new ones.
    /// Its possible to "return" items from this stack.
    /// </summary>
    [CollectionBuilder(typeof(PoolShadowStackBuilder), "Create")]
    public interface IPoolShadowStack<TModel, TID> : IReadOnlyCollection<TModel>, IAsyncEnumerable<TModel>
        where TID : notnull
        where TModel : notnull, IDataModel<TID>
    {
        #region Counts

        /// <summary>
        /// Count of <typeparamref name="TModel"/>, which can be stored in this <see cref="IPoolShadowStack{TModel, TID}"/>
        /// </summary>
        int Capacity { get; }

        #endregion

        #region Push

        /// <summary>
        /// Adding new <paramref name="model"/>.
        /// If overflow - it will replace oldest one.
        /// Can be canceled
        /// </summary>
        /// <param name="model">Not null model</param>
        /// <param name="token"></param>
        void Push(TModel model, CancellationToken token = default);
        /// <summary>
        /// Adding new <paramref name="model"/> async.
        /// If overflow - it will replace oldest one.
        /// </summary>
        /// <param name="model">Not null model</param>
        /// <param name="token"></param>
        Task PushAsync(TModel model, CancellationToken token = default);

        /// <summary>
        /// Adding new <paramref name="models"/>.
        /// If overflow - it will replace oldest one.
        /// Can be canceled
        /// </summary>
        /// <param name="models">Not null model</param>
        /// <param name="token"></param>
        void PushMany(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Adding new <paramref name="models"/> async.
        /// If overflow - it will replace oldest one.
        /// </summary>
        /// <param name="models">Not null model</param>
        /// <param name="token"></param>
        Task PushManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Adding new <paramref name="models"/> async.
        /// If overflow - it will replace oldest one.
        /// </summary>
        /// <param name="models">Not null model</param>
        /// <param name="token"></param>
        Task PushManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        #endregion

        #region Pop

        /// <summary>
        /// Pops <typeparamref name="TModel"/> with <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        TModel? Pop(TID id, CancellationToken token = default);
        /// <summary>
        /// Pops <typeparamref name="TModel"/> with <paramref name="id"/> async.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        Task<TModel?> PopAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Pops <typeparamref name="TModel"/>s with <paramref name="ids"/>.
        /// You must enumerate all returns
        /// or dispose enumerator (like using foreach statement, but elements, which not reached will not proceed)
        /// to avoid dead-lock
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        IEnumerable<TModel?> PopMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Pops <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// You must enumerate all returns
        /// or dispose enumerator (like using foreach statement, but elements, which not reached will not proceed)
        /// to avoid dead-lock
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        IAsyncEnumerable<TModel?> PopManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Pops <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// You must enumerate all returns
        /// or dispose enumerator (like using foreach statement, but elements, which not reached will not proceed)
        /// to avoid dead-lock
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        IAsyncEnumerable<TModel?> PopManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        #endregion

        #region Clear

        /// <summary>
        /// Cleans all stored <typeparamref name="TModel"/> and returns count of cleaned models
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Count of cleaned models</returns>
        int Clear(CancellationToken token = default);
        /// <summary>
        /// Cleans all stored <typeparamref name="TModel"/> and returns count of cleaned models async
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Count of cleaned models</returns>
        Task<int> ClearAsync(CancellationToken token = default);

        #endregion

        #region Searching

        /// <summary>
        /// Check, that <typeparamref name="TModel"/> with <paramref name="id"/> exist
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        bool Contains(TID id, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/> with <paramref name="id"/> exist
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        Task<bool> ContainsAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Check, that <typeparamref name="TModel"/>s with <paramref name="ids"/> exist
        /// You must enumerate all returns
        /// or dispose enumerator (like using foreach statement, but elements, which not reached will not proceed)
        /// to avoid dead-lock
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> exist
        /// You must enumerate all returns
        /// or dispose enumerator (like using foreach statement, but elements, which not reached will not proceed)
        /// to avoid dead-lock
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Check async, that <typeparamref name="TModel"/>s with <paramref name="ids"/> exist
        /// You must enumerate all returns
        /// or dispose enumerator (like using foreach statement, but elements, which not reached will not proceed)
        /// to avoid dead-lock
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>True, if contains</returns>
        IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default);

        #endregion
    }

    /// <summary>
    /// CollectionBuilder for <see cref="IPoolShadowStack{TModel, TID}"/>
    /// </summary>
    public static class PoolShadowStackBuilder
    {
        public static IPoolShadowStack<TModel, TID> Create<TModel, TID>()
            where TID : notnull
            where TModel : notnull, IDataModel<TID>
        {
            return PoolShadowStack<TModel, TID>.Empty;
        }
        public static IPoolShadowStack<TModel, TID> Create<TModel, TID>(ReadOnlySpan<TModel> models)
            where TID : notnull
            where TModel : notnull, IDataModel<TID>
        {
            if (models.Length == 0)
                return PoolShadowStack<TModel, TID>.Empty;
            else
            {
                PoolShadowStack<TModel, TID> collection = new(models.Length);

                foreach (TModel model in models)
                    collection.Push(model);

                return collection;
            }
        }
    }
}
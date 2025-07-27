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
    public interface IPoolShadowStack<TModel, TID> : IEnumerable<TModel>
        where TID : notnull
        where TModel : notnull, IDataModel<TID>
    {
        /// <summary>
        /// Count of <typeparamref name="TModel"/>, which can be stored in this <see cref="IPoolShadowStack{TModel, TID}"/>
        /// </summary>
        int Capacity { get; }
        /// <summary>
        /// Count of <typeparamref name="TModel"/>, which stored in this <see cref="IPoolShadowStack{TModel, TID}"/>
        /// </summary>
        int Count { get; }

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
        /// Can be canceled
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
        /// Can be canceled
        /// </summary>
        /// <param name="models">Not null model</param>
        /// <param name="token"></param>
        Task PushManyAsync(IEnumerable<TModel> models, CancellationToken token = default);
        /// <summary>
        /// Adding new <paramref name="models"/> async.
        /// If overflow - it will replace oldest one.
        /// Can be canceled
        /// </summary>
        /// <param name="models">Not null model</param>
        /// <param name="token"></param>
        Task PushManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default);

        /// <summary>
        /// Pops <typeparamref name="TModel"/> with <paramref name="id"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        TModel? Pop(TID id, CancellationToken token = default);
        /// <summary>
        /// Pops <typeparamref name="TModel"/> with <paramref name="id"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        Task<TModel?> PopAsync(TID id, CancellationToken token = default);

        /// <summary>
        /// Pops <typeparamref name="TModel"/>s with <paramref name="ids"/>.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        IEnumerable<TModel?> PopMany(IEnumerable<TID> ids, CancellationToken token = default);
        /// <summary>
        /// Pops <typeparamref name="TModel"/>s with <paramref name="ids"/> async.
        /// Can be canceled
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns>Poped out <typeparamref name="TModel"/> or null (default)</returns>
        IAsyncEnumerable<TModel?> PopManyAsync(IEnumerable<TID> ids, CancellationToken token = default);

        /// <summary>
        /// Cleans all stored <typeparamref name="TModel"/> and returns count of cleaned models
        /// Can be canceled.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Count of cleaned models</returns>
        int Clear(CancellationToken token = default);
        /// <summary>
        /// Cleans all stored <typeparamref name="TModel"/> and returns count of cleaned models async
        /// Can be canceled.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Count of cleaned models</returns>
        Task<int> ClearAsync(CancellationToken token = default);

        /// <summary>
        /// Copies <typeparamref name="TModel"/>s from this <see cref="IPoolShadowStack{TModel, TID}"/> to <typeparamref name="TModel"/> array.
        /// Can be canceled
        /// </summary>
        /// <param name="array">Destination array</param>
        /// <param name="index">Index in dest <paramref name="array"/></param>
        /// <param name="count">Count of <typeparamref name="TModel"/>s to copy to <paramref name="array"/></param>
        /// <param name="token"></param>
        /// <returns>Count of copied <typeparamref name="TModel"/>s</returns>
        [Obsolete("Now we dont need CopyTo - its not implemented")]
        int ToArray(TModel[] array, int index = 0, int count = -1, CancellationToken token = default);
        /// <summary>
        /// Copies <typeparamref name="TModel"/>s from this <see cref="IPoolShadowStack{TModel, TID}"/> to <typeparamref name="TModel"/> array async.
        /// Can be canceled
        /// </summary>
        /// <param name="array">Destination array</param>
        /// <param name="index">Index in dest <paramref name="array"/></param>
        /// <param name="count">Count of <typeparamref name="TModel"/>s to copy to <paramref name="array"/></param>
        /// <param name="token"></param>
        /// <returns>Count of copied <typeparamref name="TModel"/>s</returns>
        [Obsolete("Now we dont need CopyTo - its not implemented")]
        Task<int> ToArrayAsync(TModel[] array, int index = 0, int count = -1, CancellationToken token = default);
    }

    /// <summary>
    /// CollectionBuilder for <see cref="IPoolShadowStack{TModel, TID}"/>
    /// </summary>
    public static class PoolShadowStackBuilder
    {
        public static IPoolShadowStack<TModel, TID> Create<TModel, TID>()
            where TID : notnull
            where TModel : notnull, Data.IDataModelContract<TID>
        {
            return PoolShadowStack<TModel, TID>.Empty;
        }
        public static IPoolShadowStack<TModel, TID> Create<TModel, TID>(ReadOnlySpan<TModel> models)
            where TID : notnull
            where TModel : notnull, Data.IDataModelContract<TID>
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
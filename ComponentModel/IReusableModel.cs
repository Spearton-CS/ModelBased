namespace ModelBased.ComponentModel
{
    /// <summary>
    /// <see cref="IDataModel{TSelf, TID}"/>, which can be reused on <see cref="IDataModel{TSelf, TID}.Factory(TID, CancellationToken)"/>
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    /// <typeparam name="TID"></typeparam>
    public interface IReusableModel<TSelf, TID> : IDataModel<TSelf, TID>
        where TSelf : IReusableModel<TSelf, TID>
        where TID : notnull
    {
        /// <summary>
        /// Static factory-function for reusing <typeparamref name="TSelf"/>. As default used by <see cref="Collections.Generic.IModelPool{TModel, TID}"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reuse"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        abstract static TSelf Factory(TID id, TSelf? reuse, CancellationToken token = default);
        /// <summary>
        /// Static factory-function for reusing <typeparamref name="TSelf"/>. As default used by <see cref="Collections.Generic.IModelPool{TModel, TID}"/>.
        /// Works only when <see cref="IDataModel{TSelf, TID}.SupportsAsyncFactory"/>, otherwise <see cref="NotSupportedException"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reuse"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        abstract static Task<TSelf> FactoryAsync(TID id, TSelf? reuse, CancellationToken token = default);
    }
}
namespace ModelBased.ComponentModel
{
    /// <summary>
    /// Contract for DataModel. With Factory
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    /// <typeparam name="TID">ID for this Model type</typeparam>
    public interface IDataModel<TSelf, TID> : IDataModel<TID>
        where TID : notnull
        where TSelf : notnull, IDataModel<TSelf, TID>
    {
        /// <summary>
        /// True if <see cref="FactoryAsync(TID, CancellationToken)"/> will work.
        /// </summary>
        abstract static bool SupportsAsyncFactory { get; }
        /// <summary>
        /// Static factory-function. As default used by <see cref="Collections.Generic.IModelPool{TModel, TID}"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        abstract static TSelf Factory(TID id, CancellationToken token = default);
        /// <summary>
        /// Static factory-function. As default used by <see cref="Collections.Generic.IModelPool{TModel, TID}"/>.
        /// Works only when <see cref="SupportsAsyncFactory"/>, otherwise <see cref="NotSupportedException"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        abstract static Task<TSelf> FactoryAsync(TID id, CancellationToken token = default);
    }
    /// <summary>
    /// Contract for DataModel.
    /// </summary>
    /// <typeparam name="TID">ID for this Model type</typeparam>
    public interface IDataModel<TID>
        where TID : notnull
    {
        /// <summary>
        /// <typeparamref name="TID"/>, which unique between type-equal <see cref="IDataModel{TSelf, TID}"/> (e.g: Tracks with ID 0 and with ID 1)
        /// </summary>
        TID ID { get; }

        /// <summary>
        /// Is <paramref name="id"/> equal to <see cref="ID"/>?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool EqualsByID(TID id);
    }
}
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
        /// Static factory-function. As default used by <see cref="Collections.Generic.IModelPool{TModel, TID}"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        abstract static TSelf Factory(TID id);
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
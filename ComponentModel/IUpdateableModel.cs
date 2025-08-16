namespace ModelBased.ComponentModel
{
    /// <summary>
    /// <see cref="IDataModel{TID}"/>, which can be updated using other <see cref="IDataModel{TID}"/>
    /// </summary>
    /// <typeparam name="TID"></typeparam>
    public interface IUpdateableModel<TID> : IDataModel<TID>
        where TID : notnull
    {
        /// <summary>
        /// Updates this <see cref="IUpdateableModel{TID}"/> using other <see cref="IDataModel{TID}"/>
        /// </summary>
        /// <param name="other"></param>
        public void Update(IDataModel<TID> other);
    }

    /// <summary>
    /// <see cref="IDataModel{TID}"/>, which can be updated using other <typeparamref name="TSelf"/>
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    /// <typeparam name="TID"></typeparam>
    public interface IUpdateableModel<TSelf, TID> : IUpdateableModel<TID>
        where TID : notnull
        where TSelf : IUpdateableModel<TSelf, TID>
    {
        /// <summary>
        /// Updates this <see cref="IUpdateableModel{TSelf, TID}"/> using other <typeparamref name="TSelf"/>
        /// </summary>
        /// <param name="other"></param>
        public void Update(TSelf other);
    }
}
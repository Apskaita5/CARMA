namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Describes domain objects that are persisted,
    /// i.e. could be either new (not yet persisted) or old (not new, already persisted).
    /// </summary>
    public interface IPersisted : IDomainObject
    {
        /// <summary>
        /// Returns true if this is a new object, false if it is a pre-existing object.
        /// </summary>
        /// <remarks>
        /// An object is considered to be new if its primary identifying (key) value 
        /// doesn't correspond to data in the database. In other words, 
        /// if the data values in this particular object have not yet been saved to the database
        /// the object is considered to be new. Likewise, if the object's data has been deleted from the database
        /// then the object is considered to be new.
        /// </remarks>
        /// <returns>A value indicating if this object is new.</returns>
        bool IsNew { get; }
    }
}

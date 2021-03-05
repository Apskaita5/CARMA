namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// This is the core interface implemented by all domain entity classes.
    /// Domain entity must have some unique identity value (primary key) that
    /// uniquely identifies an entity within the application database graph
    /// or indicates that the entity is new, not yet added to the application database graph.
    /// </summary>
    public interface IDomainEntity : IDomainObject
    {

        /// <summary>
        /// Gets an identity object that uniquely identifies the domain entity instance
        /// within the application database graph or indicates that the entity is new,
        /// not yet added to the application database graph.
        /// </summary>
        IDomainEntityIdentity Id { get; }

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

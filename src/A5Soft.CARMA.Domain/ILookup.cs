namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// This is the core interface implemented by all lookup classes, i.e. rich references,
    /// e.g. PersonInfo instead of PersonId.
    /// Lookup must have some unique identity value (primary key) that
    /// references an entity within the application database graph.
    /// </summary>
    public interface ILookup : IDomainObject
    {
        /// <summary>
        /// Gets an identity object that uniquely identifies the domain entity instance
        /// within the application database graph that is referenced by the lookup.
        /// </summary>
        IDomainEntityIdentity Id { get; }
    }
}

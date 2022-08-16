namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A domain object that is a child of other (parent) domain object.
    /// </summary>
    public interface IChild
    {
        /// <summary>
        /// Gets a parent object if it's a child object, or null if it's a parent object.
        /// </summary>
        IDomainObject Parent { get; }
    }
}

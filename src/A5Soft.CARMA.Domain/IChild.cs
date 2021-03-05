namespace A5Soft.CARMA.Domain
{
    public interface IChild
    {
        /// <summary>
        /// Gets a parent object if it's a child object, or null if it's a parent object.
        /// </summary>
        IDomainObject Parent { get; }
    }
}

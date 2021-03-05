using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Common interface for entity level business rules.
    /// </summary>
    public interface IEntityValidationRule
    {
        /// <summary>
        /// Returns a broken rule if the business rule for the entity instance is broken.
        /// Otherwise - returns null.
        /// </summary>
        /// <param name="instance">an instance of the entity to check the rule for</param>
        /// <param name="entityInfo">the metadata of the entity to check the rule for</param>
        /// <returns>a broken rule if the business rule for the entity instance is broken,
        /// otherwise - null.</returns>
        BrokenRule GetValidationResult(object instance, IEntityMetadata entityInfo);
    }
}

using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// A base interface for validation attributes that implement entity level validation with severity.
    /// </summary>
    public interface IEntityValidationAttribute
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

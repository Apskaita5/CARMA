using System.Collections.Generic;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// A base interface for validation attributes that implement property level validation with severity.
    /// </summary>
    public interface IPropertyValidationAttribute
    {
        /// <summary>
        /// Returns a broken rule if the business rule for the entity instance is broken.
        /// Otherwise - returns null.
        /// </summary>
        /// <param name="instance">an instance of the entity to check the rule for</param>
        /// <param name="propInfo">the metadata of the property to check the rule for</param>
        /// <param name="relatedProps">the metadata of the properties that are affected (if any)</param>
        /// <returns>a broken rule if the business rule for the entity instance is broken,
        /// otherwise - null.</returns>
        BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps);
    }
}

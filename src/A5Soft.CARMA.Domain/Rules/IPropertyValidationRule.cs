using System.Collections.Generic;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Common interface for property validation business rules.
    /// </summary>
    public interface IPropertyValidationRule
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

        /// <summary>
        /// Gets a list of properties (their names) which values are used by the validation rule (if any).
        /// </summary>
        /// <returns>a list of properties (their names) which values are used by the validation rule (if any)</returns>
        List<string> GetRelatedProperties();
    }
}

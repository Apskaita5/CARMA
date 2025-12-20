using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Context passed to validation rules containing metadata and configuration.
    /// </summary>
    public interface IValidationContext
    {
        /// <summary>
        /// The entity instance being validated.
        /// </summary>
        object Instance { get; }

        /// <summary>
        /// Metadata for the property being validated (null for entity-level rules).
        /// </summary>
        IPropertyMetadata PropertyMetadata { get; }

        /// <summary>
        /// Metadata for the entity being validated.
        /// </summary>
        IEntityMetadata EntityMetadata { get; }

        /// <summary>
        /// Rule-specific metadata from the attribute.
        /// </summary>
        IRuleMetadata RuleMetadata { get; }
    }
}

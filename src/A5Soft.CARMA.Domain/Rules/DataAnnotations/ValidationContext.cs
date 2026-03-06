using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <inheritdoc cref="IValidationContext"/>
    internal class ValidationContext : IValidationContext
    {
        /// <summary>
        /// Constructor for an entity rule.
        /// </summary>
        /// <param name="instance"><see cref="IValidationContext.Instance"/> (not null)</param>
        /// <param name="entityMetadata"><see cref="IValidationContext.EntityMetadata"/> (not null)</param>
        /// <param name="ruleMetadata"><see cref="IValidationContext.RuleMetadata"/> (not null)</param>
        public ValidationContext(object instance, IEntityMetadata entityMetadata, IRuleMetadata ruleMetadata)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            EntityMetadata = entityMetadata ?? throw new ArgumentNullException(nameof(entityMetadata));
            RuleMetadata = ruleMetadata ?? throw new ArgumentNullException(nameof(ruleMetadata));
            if (!EntityMetadata.EntityType.IsAssignableFrom(Instance.GetType()))
                throw new ArgumentException($"EntityMetadata is for entity of type {EntityMetadata.EntityType.FullName} " +
                    $"while the instance is of type {Instance.GetType().FullName}.", nameof(entityMetadata));
        }

        /// <summary>
        /// Constructor for a property rule.
        /// </summary>
        /// <param name="instance"><see cref="IValidationContext.Instance"/> (not null)</param>
        /// <param name="entityMetadata"><see cref="IValidationContext.EntityMetadata"/> (not null)</param>
        /// <param name="ruleMetadata"><see cref="IValidationContext.RuleMetadata"/> (not null)</param>
        /// <param name="propertyMetadata"><see cref="IValidationContext.PropertyMetadata"/> (not null)</param>
        public ValidationContext(object instance, IEntityMetadata entityMetadata, IRuleMetadata ruleMetadata,
            IPropertyMetadata propertyMetadata) : this(instance, entityMetadata, ruleMetadata)
        {
            PropertyMetadata = propertyMetadata ?? throw new ArgumentNullException(nameof(PropertyMetadata));
            if (EntityMetadata.EntityType != PropertyMetadata.EntityType) throw new ArgumentException(
                $"EntityMetadata is for entity of type {EntityMetadata.EntityType.FullName} " +
                    $"while the PropertyMetadata is for entity of type {PropertyMetadata.EntityType.FullName}.", nameof(propertyMetadata));
        }


        /// <inheritdoc cref="IValidationContext.Instance"/>
        public object Instance { get; }

        /// <inheritdoc cref="IValidationContext.PropertyMetadata"/>
        public IPropertyMetadata PropertyMetadata { get; } = null;

        /// <inheritdoc cref="IValidationContext.EntityMetadata"/>
        public IEntityMetadata EntityMetadata { get; }

        /// <inheritdoc cref="IValidationContext.RuleMetadata"/>
        public IRuleMetadata RuleMetadata { get; }
    }
}

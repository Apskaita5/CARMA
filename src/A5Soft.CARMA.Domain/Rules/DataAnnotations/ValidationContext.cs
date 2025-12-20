using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    internal class ValidationContext : IValidationContext
    {
        public ValidationContext(object instance, IEntityMetadata entityMetadata, IRuleMetadata ruleMetadata)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            EntityMetadata = entityMetadata ?? throw new ArgumentNullException(nameof(entityMetadata));
            RuleMetadata = ruleMetadata ?? throw new ArgumentNullException(nameof(ruleMetadata));
        }

        public ValidationContext(object instance, IEntityMetadata entityMetadata, IRuleMetadata ruleMetadata,
            IPropertyMetadata propertyMetadata) : this(instance, entityMetadata, ruleMetadata)
        {
            PropertyMetadata = propertyMetadata ?? throw new ArgumentNullException(nameof(PropertyMetadata));
        }

        public object Instance { get; }
        public IPropertyMetadata PropertyMetadata { get; } = null;
        public IEntityMetadata EntityMetadata { get; }
        public IRuleMetadata RuleMetadata { get; }
    }
}

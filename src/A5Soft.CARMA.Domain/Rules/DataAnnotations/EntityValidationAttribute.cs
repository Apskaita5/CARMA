using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base attribute for entity validation rules.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class EntityValidationAttribute : Attribute, IRuleMetadata
    {
        private static readonly string[] emptyProps = new string[] { };

        /// <summary>
        /// The type of IEntityValidationRule to resolve from DI.
        /// </summary>
        public Type RuleType { get; }

        /// <summary>
        /// Severity level for this rule.
        /// </summary>
        public RuleSeverity Severity { get; }

        /// <inheritdoc cref="IRuleMetadata.NullIsValid"/>
        public bool NullIsValid => false;

        /// <inheritdoc cref="IRuleMetadata.EnabledProperty"/>
        public string EnabledProperty { get; set; } = string.Empty;

        /// <inheritdoc cref="IRuleMetadata.EnabledInversed"/>
        public bool EnabledInversed { get; set; } = false;


        protected EntityValidationAttribute(Type ruleType, RuleSeverity severity)
        {
            RuleType = ruleType ?? throw new ArgumentNullException(nameof(ruleType));
            Severity = severity;

            if (!typeof(IValidationRule).IsAssignableFrom(ruleType))
                throw new ArgumentException($"Rule type must implement IValidationRule", nameof(ruleType));
        }


        public HashSet<string> GetRelatedProperties()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base attribute for property validation rules. Acts as a rule metadata container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class PropertyValidationAttribute : Attribute, IRuleMetadata
    {
        /// <summary>
        /// Gets related prop names, i.e. props which values influence the validation.
        /// </summary>
        public string[] RelatedProperties { get; }

        /// <summary>
        /// The specific type of IValidationRule to resolve from DI.
        /// </summary>
        public Type RuleType { get; }

        /// <summary>
        /// Severity level for this rule.
        /// </summary>
        public RuleSeverity Severity { get; }


        protected PropertyValidationAttribute(Type ruleType, RuleSeverity severity, string[] relatedProperties)
        {
            RuleType = ruleType ?? throw new ArgumentNullException(nameof(ruleType));
            Severity = severity;
            RelatedProperties = relatedProperties ?? throw new ArgumentNullException(nameof(relatedProperties));

            if (!typeof(IValidationRule).IsAssignableFrom(ruleType))
                throw new ArgumentException($"Rule type must implement IValidationRule", nameof(ruleType));
        }
    }
}

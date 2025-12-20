using System;

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
        /// Gets related prop names, i.e. props which values influence the validation.
        /// </summary>
        string[] IRuleMetadata.RelatedProperties => emptyProps;

        /// <summary>
        /// The type of IEntityValidationRule to resolve from DI.
        /// </summary>
        public Type RuleType { get; }

        /// <summary>
        /// Severity level for this rule.
        /// </summary>
        public RuleSeverity Severity { get; }


        protected EntityValidationAttribute(Type ruleType, RuleSeverity severity)
        {
            if (ruleType == null) throw new ArgumentNullException(nameof(ruleType));
            if (!typeof(IValidationRule).IsAssignableFrom(ruleType))
                throw new ArgumentException($"Rule type must implement IValidationRule", nameof(ruleType));

            RuleType = ruleType;
            Severity = severity;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base attribute for property validation rules. Acts as a rule metadata container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class PropertyValidationAttribute : Attribute, IRuleMetadata
    {
        /// <inheritdoc cref="IRuleMetadata.RuleType"/>
        public Type RuleType { get; }

        /// <inheritdoc cref="IRuleMetadata.Severity"/>
        public RuleSeverity Severity { get; }

        /// <summary>
        /// Custom list of related properties, if any.
        /// </summary>
        public string[] RelatedProperties { get; set; } = null;

        /// <inheritdoc cref="IRuleMetadata.NullIsValid"/>
        public bool NullIsValid { get; set; } = true;

        /// <inheritdoc cref="IRuleMetadata.EnabledProperty"/>
        public string EnabledProperty { get; set; } = string.Empty;

        /// <inheritdoc cref="IRuleMetadata.EnabledInversed"/>
        public bool EnabledInversed { get; set; } = false;


        protected PropertyValidationAttribute(Type ruleType, RuleSeverity severity)
        {
            RuleType = ruleType ?? throw new ArgumentNullException(nameof(ruleType));
            Severity = severity;

            if (!typeof(IValidationRule).IsAssignableFrom(ruleType))
                throw new ArgumentException($"Rule type must implement IValidationRule", nameof(ruleType));
        }


        /// <summary>
        /// Gets a list of property names that the evaluated property validation depends upon.
        /// </summary>
        /// <returns>a list of property names that the evaluated property validation depends upon</returns>
        public HashSet<string> GetRelatedProperties()
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddRelatedProperties(result);
            return result;
        }

        /// <summary>
        /// Adds dependant properties for the implementation.
        /// base.AddRelatedProperties() should always be invoked.
        /// </summary>
        /// <param name="relatedProperties">a list of property names that the evaluated property validation depends upon</param>
        protected virtual void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            if (!string.IsNullOrWhiteSpace(EnabledProperty))
                relatedProperties.Add(EnabledProperty.Trim());
            if (null != RelatedProperties)
            {
                foreach (var relatedProp in RelatedProperties.Where(v => !string.IsNullOrWhiteSpace(v)))
                {
                    relatedProperties.Add(relatedProp.Trim());
                }
            }
        }
    }
}

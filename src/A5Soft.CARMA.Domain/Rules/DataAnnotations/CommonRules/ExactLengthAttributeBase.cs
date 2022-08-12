using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for exact string length validation.
    /// </summary> 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class ExactLengthAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(string) };


        /// <summary>
        ///  exact string length rule
        /// </summary>
        /// <param name="length">string length (that is valid)</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected ExactLengthAttributeBase(int length, RuleSeverity severity = RuleSeverity.Error)
            : base()
        {
            Severity = severity;
            RequiredLength = length;
        }


        /// <summary>
        /// Gets a required string length (that is valid).
        /// </summary>
        public int RequiredLength { get; }

        /// <inheritdoc/>
        protected override bool NullIsAlwaysValid => true;

        /// <inheritdoc/>
        protected override Type[] SupportedValueTypes => _supportedValueTypes;

        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var validatedValue = (string)value;

            if (validatedValue.IsNullOrWhiteSpace() || validatedValue.Trim().Length == RequiredLength) return null;

            return GetLocalizedErrorMessageFor(propertyDisplayName);
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName);
    }
}

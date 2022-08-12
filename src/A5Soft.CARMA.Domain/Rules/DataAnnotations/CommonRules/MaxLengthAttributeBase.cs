using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for max string length validation.
    /// </summary> 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class MaxLengthAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(string) };


        /// <summary>
        ///  max string length rule
        /// </summary>
        /// <param name="maximumLength">maximum string length</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected MaxLengthAttributeBase(int maximumLength, RuleSeverity severity = RuleSeverity.Error)
            : base()
        {
            Severity = severity;
            MaxLength = (uint)maximumLength;
        }


        /// <summary>
        /// Gets a max allowed string length.
        /// </summary>
        public uint MaxLength { get; }

        /// <inheritdoc/>
        protected override bool NullIsAlwaysValid => true;

        /// <inheritdoc/>
        protected override Type[] SupportedValueTypes => _supportedValueTypes;

        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var validatedValue = (string)value;

            if (validatedValue.IsNullOrWhiteSpace() || validatedValue.Trim().Length <= MaxLength) return null;

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

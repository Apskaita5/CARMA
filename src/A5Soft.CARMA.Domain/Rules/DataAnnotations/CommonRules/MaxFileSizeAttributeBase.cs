using A5Soft.CARMA.Domain.Math;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for max file size rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class MaxFileSizeAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] {
            typeof(int), typeof(long), typeof(int?), typeof(long?) };


        /// <summary>
        /// Max file size rule.
        /// </summary>
        /// <param name="maxSizeMB">max allowed file size (MB)</param>
        /// <param name="severity">a severity of the rule</param>
        protected MaxFileSizeAttributeBase(int maxSizeMB, RuleSeverity severity = RuleSeverity.Error) : base()
        {
            MaxSizeMB = maxSizeMB;
            Severity = severity;
        }


        /// <summary>
        /// Gets a max allowed file size (MB).
        /// </summary>
        public int MaxSizeMB { get; }

        /// <inheritdoc />
        protected override bool NullIsAlwaysValid => true;

        /// <inheritdoc />
        protected override Type[] SupportedValueTypes => _supportedValueTypes;

        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            long validatedValue = 0;
            if (value is int intValue) validatedValue = intValue;
            else validatedValue = (long)value;

            if (validatedValue > MaxSizeMB * 1024 * 1024)
                return GetLocalizedErrorMessageFor(ToMB(validatedValue));

            return null;
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="actualSizeMB">file size that is invalid (too large)</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(decimal actualSizeMB);

        private decimal ToMB(long value)
        {
            return (Convert.ToDecimal(value) / 1024.0m / 1024.0m).AccountingRound(2);
        }
    }
}

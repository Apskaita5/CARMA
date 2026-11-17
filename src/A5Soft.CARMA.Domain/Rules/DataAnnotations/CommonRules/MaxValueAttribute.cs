using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a max value validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MaxValueAttribute : PropertyValidationAttribute, IValueRangePropertyRuleMetadata
    {
        private int _significantDigits = 2;


        /// <summary>
        /// max decimal (double, float, int) value validation
        /// </summary>
        /// <param name="maxValue">max allowed value</param>
        /// <param name="digits">significant digits to compare double or decimal values (default - 2)</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        public MaxValueAttribute(double maxValue, int digits = 2, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IMaxValueRule), severity)
        {
            if (double.IsNaN(maxValue) || double.IsInfinity(maxValue))
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue,
                    "Value limit must be a finite number.");
            ValueLimit = maxValue;
            SignificantDigits = digits;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IValueRangePropertyRuleMetadata.ValueLimit"/>
        public double ValueLimit { get; }

        /// <inheritdoc cref="IValueRangePropertyRuleMetadata.SignificantDigits"/>
        public int SignificantDigits
        {
            get => _significantDigits;
            private set
            {
                if (value < 0 || value > 15) throw new ArgumentOutOfRangeException(nameof(SignificantDigits), value,
                    "Significant digits must be between 0 and 15.");
                _significantDigits = value;
            }
        }
    }
}

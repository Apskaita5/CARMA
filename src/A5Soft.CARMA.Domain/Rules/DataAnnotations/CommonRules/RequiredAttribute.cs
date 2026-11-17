using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a required rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequiredAttribute : PropertyValidationAttribute, IRequiredRuleMetadata
    {
        private int _significantDigits = 2;


        /// <inheritdoc />
        public RequiredAttribute(RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IRequiredRule), severity)
        {
            NullIsValid = false;
        }


        /// <summary>
        /// Gets or sets a value indicating whether the property value is a reference (primary key) of a domain entity.
        /// </summary>
        public bool IsKeyReference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an empty string is valid.
        /// Only applicable for string properties. Default is false.
        /// </summary>
        public bool AllowEmptyStrings { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether negative numeric property value is valid.
        /// Only applicable for numeric properties. Default is false.
        /// </summary>
        public bool AllowNegative { get; set; } = false;

        /// <summary>
        /// Gets or sets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        public int SignificantDigits
        {
            get => _significantDigits;
            set
            {
                if (value < 0 || value > 15) throw new ArgumentOutOfRangeException(nameof(SignificantDigits), value,
                    "Significant digits must be between 0 and 15.");
                _significantDigits = value;
            }
        }
    }
}

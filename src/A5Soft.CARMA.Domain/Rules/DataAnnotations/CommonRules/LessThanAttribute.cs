using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds less than (other property) rule to an entity property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class LessThanAttribute : PropertyValidationAttribute, INumericRefPropertyRuleMetadata
    {
        private int _significantDigits = 2;


        /// <summary>
        /// Property value is less than <see cref="ReferenceProperty"/> value rule.
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        /// <remarks>For <see langword="null"/> values rule is not applied, i.e. always returns valid result.</remarks>
        public LessThanAttribute(string referenceProperty, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(ILessThanRule), severity)
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty.Trim();
            NullIsValid = true;
        }


        /// <inheritdoc cref="IRefPropertyRuleMetadata.ReferenceProperty"/>
        public string ReferenceProperty { get; }

        /// <inheritdoc cref="INumericRefPropertyRuleMetadata.SignificantDigits"/>
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

        /// <inheritdoc cref="INumericRefPropertyRuleMetadata.EvaluateTime"/>
        public bool EvaluateTime { get; set; } = false;

        /// <inheritdoc cref="INumericRefPropertyRuleMetadata.EqualValuesAreValid"/>
        public bool EqualValuesAreValid { get; set; } = false;
    }
}

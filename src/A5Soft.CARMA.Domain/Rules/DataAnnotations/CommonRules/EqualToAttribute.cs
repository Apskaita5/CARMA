using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds equals to (other property) rule to an entity property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EqualToAttribute : PropertyValidationAttribute, IEqualToRuleMetadata
    {
        private int _significantDigits = 2;


        /// <summary>
        /// (property) values equal rule
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        public EqualToAttribute(string referenceProperty, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IEqualToRule), severity)
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty.Trim();
            NullIsValid = false;
        }


        /// <inheritdoc cref="IRefPropertyRuleMetadata.ReferenceProperty"/>
        public string ReferenceProperty { get; }

        /// <inheritdoc cref="IEqualToRuleMetadata.SignificantDigits"/>
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

        /// <inheritdoc cref="IEqualToRuleMetadata.StringComparison"/>
        public StringComparison StringComparison { get; set; } = StringComparison.CurrentCulture;
    }
}

using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds less than (other property) rule to an entity property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class LessThanAttribute : PropertyValidationAttribute, INumericRefPropertyRuleMetadata
    {
        /// <summary>
        /// Property value is greater than <see cref="ReferenceProperty"/> value rule.
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        /// <remarks>For <see langword="null"/> values rule is not applied, i.e. always returns valid result.</remarks>
        public LessThanAttribute(string referenceProperty, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(ILessThanRule), severity)
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IRefPropertyRuleMetadata.ReferenceProperty"/>
        public string ReferenceProperty { get; }

        /// <inheritdoc cref="INumericRefPropertyRuleMetadata.SignificantDigits"/>
        public int SignificantDigits { get; set; } = 2;

        /// <inheritdoc cref="INumericRefPropertyRuleMetadata.EvaluateTime"/>
        public bool EvaluateTime { get; set; } = false;

        /// <inheritdoc cref="INumericRefPropertyRuleMetadata.EqualValuesAreValid"/>
        public bool EqualValuesAreValid { get; set; } = false;


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
            relatedProperties.Add(ReferenceProperty.Trim());
        }
    }
}

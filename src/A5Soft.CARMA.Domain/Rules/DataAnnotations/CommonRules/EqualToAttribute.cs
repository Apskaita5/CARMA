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
        /// <summary>
        /// (property) values equal rule
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        public EqualToAttribute(string referenceProperty, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IEqualToRule), severity)
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty;
            NullIsValid = false;
        }


        /// <inheritdoc cref="IRefPropertyRuleMetadata.ReferenceProperty"/>
        public string ReferenceProperty { get; }

        /// <inheritdoc cref="IEqualToRuleMetadata.SignificantDigits"/>
        public int SignificantDigits { get; set; } = 2;

        /// <inheritdoc cref="IEqualToRuleMetadata.StringComparison"/>
        public StringComparison StringComparison { get; set; } = StringComparison.CurrentCulture;


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
            relatedProperties.Add(ReferenceProperty.Trim());
        }
    }
}

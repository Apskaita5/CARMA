using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds at least one property value required rule to an entity property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AtLeastOneRequiredAttribute : PropertyValidationAttribute, IAtLeastOneRequiredRuleMetadata
    {
        /// <summary>
        /// Creates a new instance of AtLeastOneRequired rule.
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        public AtLeastOneRequiredAttribute(string referenceProperty, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IAtLeastOneRequiredRule), severity)
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty;
            NullIsValid = false;
        }


        /// <summary>
        /// Gets a name of the other property to check.
        /// </summary>
        public string ReferenceProperty { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the property value is a reference (primary key) of a domain entity.
        /// </summary>
        public bool IsKeyReference { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether negative numeric property value is valid.
        /// Only applicable for numeric properties. Default is false.
        /// </summary>
        public bool AllowNegative { get; set; } = false;

        /// <summary>
        /// Gets or sets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        public int SignificantDigits { get; set; } = 2;


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
            relatedProperties.Add(ReferenceProperty.Trim());
        }
    }
}

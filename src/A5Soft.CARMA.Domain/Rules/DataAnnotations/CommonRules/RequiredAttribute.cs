using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a required rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequiredAttribute : PropertyValidationAttribute, IRequiredRuleMetadata
    {
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
        public int SignificantDigits { get; set; } = 2;


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
        }
    }
}

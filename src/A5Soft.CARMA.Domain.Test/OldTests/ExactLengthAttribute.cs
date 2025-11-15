using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace A5Soft.CARMA.Domain.Test
{
    internal class ExactLengthAttribute : ExactLengthAttributeBase
    {
        public ExactLengthAttribute(int length, RuleSeverity severity = RuleSeverity.Error) 
            : base(length, severity) {}

        protected override string GetLocalizedErrorMessageFor(string localizedPropName)
        {
            return $"{localizedPropName} length should be {this.RequiredLength} characters.";
        }

        public override bool IsValid(object value)
        {
            return base.IsValid(value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return base.IsValid(value, validationContext);
        }

        public override BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo, IEnumerable<IPropertyMetadata> relatedProps)
        {
            return base.GetValidationResult(instance, propInfo, relatedProps);
        }
    }
}

using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for date sanity rule, i.e. not in future and not too far in past (20 years or more).
    /// Can use it as is by setting ErrorMessageResourceType and ErrorMessageResourceName values
    /// in the attribute decorator or inherit this class and set ErrorMessageResourceType and ErrorMessageResourceName
    /// in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DateSanityAttribute : ValidationAttribute, IPropertyValidationAttribute
    {

        /// <summary>
        /// Date sanity rule, i.e. not in future and not too far in past (20 years or more).
        /// </summary>
        public DateSanityAttribute()
            :base() { }


        /// <summary>
        /// Gets or sets a value indicating severity of broken rule. Default - Warning.
        /// </summary>
        public RuleSeverity Severity { get; set; } = RuleSeverity.Warning;


        /// <inheritdoc cref="System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid" />
        public override bool IsValid(object value)
        {
            if (Severity == RuleSeverity.Error) return IsValidInternal(value);
            return true;
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (IsValidInternal(propInfo.GetValue(instance))) return null;

            return new BrokenRule(this.GetType().FullName, propInfo.Name, string.Format(
                CultureInfo.CurrentCulture, this.ErrorMessageString, propInfo.GetDisplayName()), Severity);
        }


        private bool IsValidInternal(object value)
        {
            if (value.IsNull()) return true;

            if (value is DateTime dateValue)
                return dateValue.Date <= DateTime.Today.Date 
                    && dateValue.Date > DateTime.Today.AddYears(-20).Date;
            
            throw new InvalidOperationException("DateSanityAttribute can only be applied to DateTime properties.");
        }

    }
}

using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for date sanity rule, i.e. not in future and not too far in past (20 years or more).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class DateSanityAttributeBase : ValidationAttribute, IPropertyValidationAttribute
    {
        /// <summary>
        /// Gets a value indicating severity of broken rule. Default - Warning.
        /// </summary>
        public RuleSeverity Severity { get; }

        /// <inheritdoc />
        public override bool RequiresValidationContext
            => true;


        /// <summary>
        /// Date sanity rule, i.e. not in future and not too far in past (20 years or more).
        /// </summary>
        /// <param name="severity">a value indicating severity of broken rule. Default - Warning.</param>
        protected DateSanityAttributeBase(RuleSeverity severity = RuleSeverity.Warning)
            : base()
        {
            Severity = severity;
        }


        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (Severity != RuleSeverity.Error || null == value) return true;

            if (value is DateTime dateValue)
            {
                return dateValue.Date <= DateTime.UtcNow.Date && dateValue.Date > DateTime.Today.AddYears(-20).Date;
            }

            throw new InvalidOperationException("DateSanityAttribute can only be applied to DateTime properties.");
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            var value = propInfo.GetValue(instance);

            if (null == value) return null;

            if (value is DateTime dateValue)
            {
                if (dateValue.Date > DateTime.UtcNow.Date) return new BrokenRule(this.GetType().FullName, 
                    propInfo.Name, GetInFutureLocalizedErrorMessageFor(propInfo.GetDisplayName()), Severity);
                if (dateValue.Date < DateTime.Today.AddYears(-20).Date) return new BrokenRule(this.GetType().FullName,
                    propInfo.Name, GetTooOldLocalizedErrorMessageFor(propInfo.GetDisplayName()), Severity);
                return null;
            }

            throw new InvalidOperationException($"DateSanityAttribute can only be applied to DateTime properties while {propInfo.Name} is not.");
        }


        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (null == validationContext) throw new ArgumentNullException(nameof(validationContext));

            if (Severity != RuleSeverity.Error || null == value) return null;

            if (value is DateTime dateValue)
            {
                if (dateValue.Date > DateTime.UtcNow.Date)
                    return new ValidationResult(GetInFutureLocalizedErrorMessageFor(
                        GetPropertyDisplayName(validationContext)));
                if (dateValue.Date < DateTime.Today.AddYears(-20).Date) 
                    return new ValidationResult(GetTooOldLocalizedErrorMessageFor(
                        GetPropertyDisplayName(validationContext)));

                return null;
            }

            throw new InvalidOperationException($"DateSanityAttribute can only be applied to " +
                $"DateTime properties while {validationContext.MemberName} is not.");
        }

        /// <summary>
        /// Implement this method to get a localized error message if the value is too old for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetTooOldLocalizedErrorMessageFor(string localizedPropName);

        /// <summary>
        /// Implement this method to get a localized error message if the value is in future for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetInFutureLocalizedErrorMessageFor(string localizedPropName);


        private string GetPropertyDisplayName(ValidationContext validationContext)
        {
            return validationContext.DisplayName.IsNullOrWhiteSpace()
                ? validationContext.GetPropertyDisplayName()
                : validationContext.DisplayName;
        }

    }
}

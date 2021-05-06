using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for min string length validation.
    /// </summary>
    public abstract class MinLengthAttributeBase : System.ComponentModel.DataAnnotations.StringLengthAttribute,
        IPropertyValidationAttribute
    {
        /// <summary>
        /// Gets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; }

        /// <inheritdoc />
        public override bool RequiresValidationContext
            => true;


        /// <summary>
        ///  min string length rule
        /// </summary>
        /// <param name="minimumLength">minimum required string length</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected MinLengthAttributeBase(int minimumLength, RuleSeverity severity = RuleSeverity.Error) 
            : base(int.MaxValue)
        {
            Severity = severity;
            MinimumLength = minimumLength;
        }


        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (Severity == RuleSeverity.Error) return IsValidInternal(value as string);
            return true;
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (IsValidInternal(propInfo.GetValue(instance) as string)) return null;

            return new BrokenRule(this.GetType().FullName, propInfo.Name,
                GetLocalizedErrorMessageFor(propInfo.GetDisplayName()), Severity);
        }


        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (null == validationContext) throw new ArgumentNullException(nameof(validationContext));

            if (Severity != RuleSeverity.Error || IsValidInternal(value as string)) return null;

            var displayName = validationContext.DisplayName.IsNullOrWhiteSpace()
                ? validationContext.GetPropertyDisplayName()
                : validationContext.DisplayName;

            return new ValidationResult(GetLocalizedErrorMessageFor(displayName));
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName);

        private bool IsValidInternal(string value)
        {
            return (value?.Trim().Length ?? 0) >= MinimumLength;
        }

    }
}

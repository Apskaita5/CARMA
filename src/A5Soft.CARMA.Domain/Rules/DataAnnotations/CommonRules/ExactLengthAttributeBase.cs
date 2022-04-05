using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for exact string length validation.
    /// </summary> 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class ExactLengthAttributeBase : System.ComponentModel.DataAnnotations.StringLengthAttribute,
        IPropertyValidationAttribute
    {
        /// <summary>
        /// Gets or sets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; }

        /// <inheritdoc />
        public override bool RequiresValidationContext
            => true;


        /// <summary>
        ///  exact string length rule
        /// </summary>
        /// <param name="length">string length (that is valid)</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected ExactLengthAttributeBase(int length, RuleSeverity severity = RuleSeverity.Error)
            : base(length)
        {
            Severity = severity;
            MinimumLength = length;
        }


        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (Severity == RuleSeverity.Error) return IsValidInternal(value as string);
            return true;
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public virtual BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<Metadata.IPropertyMetadata> relatedProps)
        {
            if (IsValidInternal(propInfo.GetValue(instance)?.ToString())) return null;

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
            if (value.IsNullOrWhiteSpace()) return true;
            return value.Trim().Length == MaximumLength;
        }

    }
}

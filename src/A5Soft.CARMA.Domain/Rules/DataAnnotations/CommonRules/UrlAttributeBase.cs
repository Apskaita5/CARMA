using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for url value rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class UrlAttributeBase : DataTypeAttribute
    {
        /// <summary>
        /// Gets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; }


        /// <summary>
        /// url format rule
        /// </summary>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected UrlAttributeBase(RuleSeverity severity = RuleSeverity.Error) : base(DataType.Url)
        {
            Severity = severity;
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
            if (value.IsNullOrWhiteSpace()) return true;
            value = value.Trim();
            return value.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                   || value.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                   || value.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase);
        }

    }
}

using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for string length validation.
    /// Implementation shall set ErrorMessageResourceType in the constructor and override
    /// abstract methods to provide different resource property names for different types of errors.
    /// </summary>
    public abstract class StringLengthAttribute : System.ComponentModel.DataAnnotations.StringLengthAttribute,
        IPropertyValidationAttribute
    {

        /// <summary>
        /// Gets or sets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; set; } = RuleSeverity.Error;


        protected StringLengthAttribute(int maximumLength)
            : base(maximumLength)
        {
            this.ErrorMessageResourceName = ErrorMessageResourceNameForMaxLength;
        }

        protected StringLengthAttribute(int minimumLength, int maximumLength)
            : base(maximumLength)
        {
            if (minimumLength > 0)
            {
                this.MinimumLength = minimumLength;
                if (minimumLength == maximumLength)
                {
                    this.ErrorMessageResourceName = nameof(ErrorMessageResourceNameForExact);
                }
                else
                {
                    this.ErrorMessageResourceName = ErrorMessageResourceNameForRange;
                }
            }
            else
            {
                this.ErrorMessageResourceName = ErrorMessageResourceNameForMaxLength;
            }
        }


        /// <summary>
        /// Provide resource property name for error message if string length is greater than allowed.
        /// Parameter sequence is: localized property name, min length, max length.
        /// </summary>
        protected abstract string ErrorMessageResourceNameForMaxLength { get; }

        /// <summary>
        /// Provide resource property name for error message if string length is greater or smaller than allowed.
        /// Parameter sequence is: localized property name, min length, max length.
        /// </summary>
        protected abstract string ErrorMessageResourceNameForRange { get; }

        /// <summary>
        /// Provide resource property name for error message if string length is greater or smaller
        /// than allowed exact length (i.e. when min value is equal to max value).
        /// Parameter sequence is: localized property name, min length, max length.
        /// </summary>
        protected abstract string ErrorMessageResourceNameForExact { get; }


        /// <inheritdoc cref="System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid" />
        public override bool IsValid(object value)
        {
            if (Severity != RuleSeverity.Error) return true;
            return IsValidInternal(value as string);
        }

        /// <inheritdoc cref="System.ComponentModel.DataAnnotations.ValidationAttribute.FormatErrorMessage" />
        public override string FormatErrorMessage(string name)
        {
            // it's ok to pass in the minLength even for the error message without a {2} param since String.Format will just
            // ignore extra arguments
            return String.Format(CultureInfo.CurrentCulture, this.ErrorMessageString, name, this.MinimumLength, this.MaximumLength);
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (IsValidInternal(propInfo.GetValue(instance) as string)) return null;

            return new BrokenRule(this.GetType().FullName, propInfo.Name, 
                string.Format(CultureInfo.CurrentCulture, this.ErrorMessageString, 
                    propInfo.GetDisplayName(), MinimumLength, MaximumLength), Severity);
        }


        private bool IsValidInternal(string value)
        {
            if (value.IsNullOrWhiteSpace()) return true;
            var length = value.Trim().Length;
            return length <= MaximumLength && length >= MinimumLength;
        }

    }
}

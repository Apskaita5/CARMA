using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{

    /// <summary>
    /// A base class for email rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class EmailAddressAttributeBase : System.ComponentModel.DataAnnotations.DataTypeAttribute, 
        IPropertyValidationAttribute
    {

        /// <summary>
        /// Gets or sets a value indicating whether multiple email addresses can be specified delimited by ;.
        /// </summary>
        public bool AllowMultipleAddresses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; set; }

        /// <inheritdoc />
        public override bool RequiresValidationContext
            => true;


        /// <summary>
        /// Email validation rule
        /// </summary>
        protected EmailAddressAttributeBase() : base(DataType.EmailAddress)
        { }


        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (Severity != RuleSeverity.Error) return true;
            return IsValidInternal(value as string);
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
            if (value.IsNullOrWhiteSpace()) return false;

            if (AllowMultipleAddresses)
            {
                foreach (var emailAddress in value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!IsValidEmailAddress(emailAddress.Trim())) return false;
                }
            }
            else return IsValidEmailAddress(value);

            return true;
        }

        private static bool IsValidEmailAddress(string emailAddress)
        {
            int atCount = 0;

            foreach (char c in emailAddress)
            {
                if (c == '@')
                {
                    atCount++;
                }
            }

            return (atCount == 1 && emailAddress[0] != '@' && emailAddress[emailAddress.Length - 1] != '@');
        }
        
    }
}

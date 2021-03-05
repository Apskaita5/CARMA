using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{

    /// <summary>
    /// A base class for email rule.
    /// Can use it as is by setting ErrorMessageResourceType and ErrorMessageResourceName values
    /// in the attribute decorator or inherit this class and set ErrorMessageResourceType and ErrorMessageResourceName
    /// in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EmailAddressAttribute : System.ComponentModel.DataAnnotations.DataTypeAttribute, 
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


        public EmailAddressAttribute() : base(System.ComponentModel.DataAnnotations.DataType.EmailAddress)
        { }


        /// <inheritdoc cref="System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid" />
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

            return new BrokenRule(this.GetType().FullName, propInfo.Name, string.Format(
                CultureInfo.CurrentCulture, this.ErrorMessageString, propInfo.GetDisplayName()), Severity);
        }


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

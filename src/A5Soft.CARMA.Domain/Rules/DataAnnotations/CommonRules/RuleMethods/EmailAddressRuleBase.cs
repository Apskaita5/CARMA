using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of an email validation rule.
    /// </summary>
    public abstract class EmailAddressRuleBase : PropertyValidationRuleBase<IEmailAddressRuleMetadata>,
        IEmailAddressRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IEmailAddressRuleMetadata ruleMetadata)
        {
            if (propertyMetadata.PropertyType != typeof(string)) throw new ArgumentException(
                $"Cannot handle non string values ({propertyMetadata.Name}: {propertyMetadata.PropertyType.FullName}).",
                nameof(propertyMetadata));

            if (value is string validatedEmail)
            {
                if (validatedEmail.IsNullOrWhiteSpace()) return null;

                if (IsValidEmailAddress(validatedEmail, ruleMetadata.AllowMultipleAddresses)) return null;

                return GetLocalizedErrorMessageFor(propertyMetadata.GetDisplayName());
            }

            return null;
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName);

        protected static bool IsValidEmailAddress(string emailAddress, bool allowMultiple)
        {
            if (allowMultiple)
            {
                foreach (var emailAddressItem in emailAddress.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!IsValidEmailAddress(emailAddressItem.Trim()))
                    {
                        return false;
                    }
                }
                return true;
            }
            else return IsValidEmailAddress(emailAddress);
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

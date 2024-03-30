using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for email rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class EmailAddressAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(string) };


        /// <summary>
        /// Email validation rule
        /// </summary>
        protected EmailAddressAttributeBase() : base() { }


        /// <summary>
        /// Gets or sets a value indicating whether multiple email addresses can be specified delimited by ;.
        /// </summary>
        public bool AllowMultipleAddresses { get; set; }


        /// <inheritdoc/>
        protected override bool NullIsAlwaysValid => true;

        /// <inheritdoc/>
        protected override Type[] SupportedValueTypes => _supportedValueTypes;

        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var email = (string)value;

            if (email.IsNullOrWhiteSpace()) return null;

            if (IsValidEmailAddress(email, AllowMultipleAddresses)) return null;

            return GetLocalizedErrorMessageFor(propertyDisplayName);
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

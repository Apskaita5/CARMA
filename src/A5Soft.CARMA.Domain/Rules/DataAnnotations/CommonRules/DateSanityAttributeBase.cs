using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for date sanity rule, i.e. not in future and not too far in past (20 years or more).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class DateSanityAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[]
            { typeof(DateTime), typeof(DateTime?) };


        /// <summary>
        /// Date sanity rule, i.e. not in future and not too far in past (20 years or more).
        /// </summary>
        /// <param name="severity">Rule severity, default - <see cref="RuleSeverity.Warning"/>.</param>
        protected DateSanityAttributeBase(RuleSeverity severity = RuleSeverity.Warning) : base()
        {
            Severity = severity;
        }


        /// <inheritdoc/>
        protected override bool NullIsAlwaysValid => true;

        /// <inheritdoc/>
        protected override Type[] SupportedValueTypes => _supportedValueTypes;

        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var validatedDate = (DateTime)value;

            if (validatedDate.Date > DateTime.UtcNow.Date)
                return GetInFutureLocalizedErrorMessageFor(propertyDisplayName);
            if (validatedDate.Date < DateTime.Today.AddYears(-20).Date)
                return GetTooOldLocalizedErrorMessageFor(propertyDisplayName);

            return null;
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
    }
}

using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of a date sanity check rule.
    /// </summary>
    public abstract class DateSanityRuleBase : PropertyValidationRuleBase<IRuleMetadata>,
        IDateSanityRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IRuleMetadata ruleMetadata)
        {
            if (propertyMetadata.PropertyType != typeof(DateTime) &&
                propertyMetadata.PropertyType != typeof(DateTime?)) throw new ArgumentException(
                    $"Cannot handle non DateTime values ({propertyMetadata.Name}: {propertyMetadata.PropertyType.FullName}).",
                    nameof(propertyMetadata));

            if (value is DateTime validatedDate)
            {
                if (validatedDate.Date > DateTime.UtcNow.Date)
                    return GetInFutureLocalizedErrorMessageFor(propertyMetadata.GetDisplayName());
                if (validatedDate.Date < DateTime.Today.AddYears(-20).Date)
                    return GetTooOldLocalizedErrorMessageFor(propertyMetadata.GetDisplayName());
            }

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

using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of a max file size validation rule.
    /// </summary>
    public abstract class MaxFileSizeRuleBase : PropertyValidationRuleBase<IMaxFileSizeRuleMetadata>,
        IMaxFileSizeRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IMaxFileSizeRuleMetadata ruleMetadata)
        {
            if (null == value) return null;

            if (value is int intSize)
            {
                if (intSize > ruleMetadata.MaxSizeMB * 1024 * 1024)
                    return GetLocalizedErrorMessageFor(ToMB(intSize), ruleMetadata.MaxSizeMB);
            }
            else if (value is long longSize)
            {
                if (longSize > ruleMetadata.MaxSizeMB * 1024 * 1024)
                    return GetLocalizedErrorMessageFor(ToMB(longSize), ruleMetadata.MaxSizeMB);
            }
            else
            {
                if (propertyMetadata.PropertyType != typeof(string)) throw new ArgumentException(
                $"Cannot handle non string values ({propertyMetadata.Name}: {propertyMetadata.PropertyType.FullName}).",
                nameof(propertyMetadata));
            }

            return null;
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="actualSizeMB">file size that is invalid (too large)</param>
        /// <param name="maxSizeMB">max file size allowed</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(decimal actualSizeMB, int maxSizeMB);

        private decimal ToMB(long value)
        {
            return (Convert.ToDecimal(value) / 1024.0m / 1024.0m).AccountingRound(2);
        }
    }
}

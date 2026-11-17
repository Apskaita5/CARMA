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

            long size;
            switch (value)
            {
                case int intSize: size = intSize; break;
                case long longSize: size = longSize; break;
                default:
                    throw new InvalidOperationException(
               $"Max file size rule is only applicable for int or long properties " +
               $"({propertyMetadata.Name}: {propertyMetadata.PropertyType.FullName}).");
            }

            var maxBytes = ruleMetadata.MaxSizeMB * 1024L * 1024L;
            if (size > maxBytes) return GetLocalizedErrorMessageFor(ToMB(size), ruleMetadata.MaxSizeMB);
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

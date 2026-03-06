using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of an exact (string) length validation rule.
    /// </summary>
    public abstract class ExactLengthRuleBase : PropertyValidationRuleBase<IStringLengthRuleMetadata>,
        IExactLengthRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IStringLengthRuleMetadata ruleMetadata)
        {
            if (propertyMetadata.PropertyType != typeof(string)) throw new ArgumentException(
                $"Cannot handle non string values ({propertyMetadata.Name}: {propertyMetadata.PropertyType.FullName}).",
                nameof(propertyMetadata));

            if (value is string validatedValue)
            {
                if (validatedValue.IsNullOrWhiteSpace() || validatedValue.Trim().Length == ruleMetadata.LengthLimit) return null;

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
    }
}

using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of a max value validation rule.
    /// </summary>
    public abstract class MaxValueRuleBase : PropertyValidationRuleBase<IValueRangePropertyRuleMetadata>,
        IMaxValueRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IValueRangePropertyRuleMetadata ruleMetadata)
        {
            if (ValueHelpers.CompareToLimit(value, ruleMetadata.ValueLimit, ruleMetadata.SignificantDigits) > 0)
                return GetLocalizedErrorMessageFor(propertyMetadata.GetDisplayName(),
                    ValueHelpers.FormatLimit(propertyMetadata.PropertyType, ruleMetadata.ValueLimit,
                    ruleMetadata.SignificantDigits, false));
            return null;
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName, string maxValue);
    }
}

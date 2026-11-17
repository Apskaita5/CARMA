using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of a min value validation rule.
    /// </summary>
    public abstract class MinValueRuleBase : PropertyValidationRuleBase<IValueRangePropertyRuleMetadata>,
        IMinValueRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IValueRangePropertyRuleMetadata ruleMetadata)
        {
            if (ValueHelpers.CompareToLimit(value, ruleMetadata.ValueLimit, ruleMetadata.SignificantDigits) < 0)
                return GetLocalizedErrorMessageFor(propertyMetadata.GetDisplayName(),
                ValueHelpers.FormatLimit(propertyMetadata.PropertyType, ruleMetadata.ValueLimit,
                ruleMetadata.SignificantDigits, true));
            return null;
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName, string minValue);
    }
}

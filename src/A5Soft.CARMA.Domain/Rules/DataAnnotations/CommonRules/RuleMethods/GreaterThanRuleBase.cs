using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of greater than (other property) rule using validation attributes.
    /// </summary>
    public abstract class GreaterThanRuleBase : PropertyValidationRuleBase<INumericRefPropertyRuleMetadata>,
        IGreaterThanRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, INumericRefPropertyRuleMetadata ruleMetadata)
        {
            if (!ValueHelpers.TryGetOperands(instance, value, entityMetadata, propertyMetadata,
                ruleMetadata, out var op))
                return null;

            var comparison = ValueHelpers.CompareValues(op, ruleMetadata.SignificantDigits,
                ruleMetadata.EvaluateTime);
            if (comparison > 0 || (comparison == 0 && ruleMetadata.EqualValuesAreValid)) return null;

            return GetLocalizedErrorMessageFor(propertyMetadata.GetDisplayName(), op.ReferenceProperty.GetDisplayName());
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <param name="localizedOtherPropName">a localized name of the other property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName, string localizedOtherPropName);
    }
}

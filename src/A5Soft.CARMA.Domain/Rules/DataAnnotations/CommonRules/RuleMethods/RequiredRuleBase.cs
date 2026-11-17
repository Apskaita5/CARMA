using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of a required rule.
    /// </summary>
    public abstract class RequiredRuleBase : PropertyValidationRuleBase<IRequiredRuleMetadata>,
        IRequiredRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IRequiredRuleMetadata ruleMetadata)
        {
            if (ValueHelpers.IsValidValue(value, ruleMetadata.IsKeyReference,
                ruleMetadata.AllowNegative, ruleMetadata.SignificantDigits,
                ruleMetadata.AllowEmptyStrings))
                return null;

            return GetLocalizedErrorMessageFor(propertyMetadata.GetDisplayName());
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName);
    }
}

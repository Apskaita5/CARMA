using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of equal to (other property) rule using validation attributes.
    /// </summary>
    public abstract class EqualToRuleBase : PropertyValidationRuleBase<IEqualToRuleMetadata>,
        IEqualToRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IEqualToRuleMetadata ruleMetadata)
        {
            if (!ValueHelpers.TryGetOperands(instance, value, entityMetadata, propertyMetadata, ruleMetadata, out var op))
                return null;

            if (ValueHelpers.ValuesEqual(op, ruleMetadata.SignificantDigits, ruleMetadata.StringComparison))
                return null;

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

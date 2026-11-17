using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of at least one required rule using validation attributes.
    /// </summary>
    public abstract class AtLeastOneRequiredRuleBase : PropertyValidationRuleBase<IAtLeastOneRequiredRuleMetadata>,
        IAtLeastOneRequiredRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IAtLeastOneRequiredRuleMetadata ruleMetadata)
        {
            if (string.IsNullOrWhiteSpace(ruleMetadata.ReferenceProperty)) throw new ArgumentException(
                "ReferenceProperty cannot be null.");

            if (ValueHelpers.IsValidValue(value, ruleMetadata.IsKeyReference,
                ruleMetadata.AllowNegative, ruleMetadata.SignificantDigits, false))
                return null;

            var otherProp = entityMetadata.GetPropertyMetadata(ruleMetadata.ReferenceProperty);
            if (null == otherProp) throw new ArgumentException($"No metadata for property {ruleMetadata.ReferenceProperty} " +
                $"on type {entityMetadata.EntityType.FullName}.", nameof(ruleMetadata));
            var otherValue = otherProp.GetValue(instance);

            if (ValueHelpers.IsValidValue(otherValue, ruleMetadata.IsKeyReference,
                ruleMetadata.AllowNegative, ruleMetadata.SignificantDigits, false))
                return null;

            return GetLocalizedErrorMessageFor(propertyMetadata.GetDisplayName(), otherProp.GetDisplayName());
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

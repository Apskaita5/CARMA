using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Orm;
using System;
using System.Collections;

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

            if (IsValueValid(value, ruleMetadata)) return null;

            var otherProp = entityMetadata.GetPropertyMetadata(ruleMetadata.ReferenceProperty);
            if (null == otherProp) throw new ArgumentException($"No metadata for property {ruleMetadata.ReferenceProperty} " +
                $"on type {entityMetadata.EntityType.FullName}.", nameof(ruleMetadata));
            var otherValue = otherProp.GetValue(instance);

            if (IsValueValid(otherValue, ruleMetadata)) return null;

            return GetLocalizedErrorMessageFor(propertyMetadata.GetDisplayName(), otherProp.GetDisplayName());
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <param name="localizedOtherPropName">a localized name of the other property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName, string localizedOtherPropName);

        private bool IsValueValid(object value, IAtLeastOneRequiredRuleMetadata ruleMetadata)
        {
            if (null == value) return false; // handles nullable types that does not have a value as well

            bool isValid = true;
            if (ruleMetadata.IsKeyReference)
            {
                if (value is string val) isValid = val.IsValidKey();
                else if (value is int intVal) isValid = intVal.IsValidKey();
                else if (value is long longVal) isValid = longVal.IsValidKey();
                else if (value is Guid guidVal) isValid = guidVal.IsValidKey();
                else throw new NotSupportedException(
                    $"Reference key type {value.GetType().FullName} is not supported by {this.GetType().FullName}.");
            }
            else if (value is string val)
            {
                isValid = !val.IsNullOrWhiteSpace();
            }
            else if (value is decimal decimalValue)
            {
                if (ruleMetadata.AllowNegative) isValid = decimalValue.AccountingRound(ruleMetadata.SignificantDigits) != 0.0m;
                else isValid = decimalValue.AccountingRound(ruleMetadata.SignificantDigits) > 0.0m;
            }
            else if (value is double doubleValue)
            {
                if (ruleMetadata.AllowNegative) isValid = !doubleValue.EqualsTo(0.0, ruleMetadata.SignificantDigits);
                else isValid = doubleValue.GreaterThan(0.0, ruleMetadata.SignificantDigits);
            }
            else if (value is byte byteValue)
            {
                if (ruleMetadata.AllowNegative) isValid = byteValue != 0;
                else isValid = byteValue > 0;
            }
            else if (value is short int16Value)
            {
                if (ruleMetadata.AllowNegative) isValid = int16Value != 0;
                else isValid = int16Value > 0;
            }
            else if (value is int intValue)
            {
                if (ruleMetadata.AllowNegative) isValid = intValue != 0;
                else isValid = intValue > 0;
            }
            else if (value is long longValue)
            {
                if (ruleMetadata.AllowNegative) isValid = longValue != 0;
                else isValid = longValue > 0;
            }
            else if (value is IList list)
            {
                isValid = list.Count > 0;
            }

            return isValid;
        }
    }
}

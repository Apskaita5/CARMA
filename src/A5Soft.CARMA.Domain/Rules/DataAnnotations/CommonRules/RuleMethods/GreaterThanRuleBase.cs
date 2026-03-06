using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;
using System;

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
            if (string.IsNullOrWhiteSpace(ruleMetadata.ReferenceProperty)) throw new ArgumentException(
                "ReferenceProperty cannot be null.");

            var otherProp = entityMetadata.GetPropertyMetadata(ruleMetadata.ReferenceProperty);
            if (null == otherProp) throw new ArgumentException($"No metadata for property {ruleMetadata.ReferenceProperty} " +
                $"on type {entityMetadata.EntityType.FullName}.", nameof(ruleMetadata));

            var otherValue = otherProp.GetValue(instance);

            if (IsValidInternal(value, otherValue, instance.GetType(), propertyMetadata.Name, otherProp.Name, ruleMetadata))
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

        private bool IsValidInternal(object value, object otherValue, Type entityType, string propName,
            string otherPropName, INumericRefPropertyRuleMetadata ruleMetadata)
        {
            // Do not compare null values.
            if (null == value || null == otherValue) return true;

            if (value.GetType() != otherValue.GetType()) throw new InvalidOperationException(
                $"Property value types on entity {entityType.FullName} are not the same: {propName}:" +
                $" {value.GetType().FullName} vs. {otherPropName}: {otherValue.GetType().FullName}.");

            if (value is double doubleValue)
            {
                if (otherValue is double doubleOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return doubleValue.GreaterThan(doubleOtherValue, ruleMetadata.SignificantDigits) ||
                            doubleValue.EqualsTo(doubleOtherValue, ruleMetadata.SignificantDigits);
                    }
                    else
                    {
                        return doubleValue.GreaterThan(doubleOtherValue, ruleMetadata.SignificantDigits);
                    }
                }
            }
            else if (value is float floatValue)
            {
                if (otherValue is float floatOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return floatValue - floatOtherValue >= float.Epsilon;
                    }
                    else
                    {
                        return floatValue - floatOtherValue > float.Epsilon;
                    }
                }
            }
            else if (value is decimal decimalValue)
            {
                if (otherValue is decimal decimalOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return decimalValue.AccountingRound(ruleMetadata.SignificantDigits) >= decimalOtherValue.AccountingRound(ruleMetadata.SignificantDigits);
                    }
                    else
                    {
                        return decimalValue.AccountingRound(ruleMetadata.SignificantDigits) > decimalOtherValue.AccountingRound(ruleMetadata.SignificantDigits);
                    }
                }
            }
            else if (value is DateTime dateTimeValue)
            {
                if (otherValue is DateTime dateTimeOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        if (ruleMetadata.EvaluateTime) return dateTimeValue >= dateTimeOtherValue;
                        return dateTimeValue.Date >= dateTimeOtherValue.Date;
                    }
                    else
                    {
                        if (ruleMetadata.EvaluateTime) return dateTimeValue > dateTimeOtherValue;
                        return dateTimeValue.Date > dateTimeOtherValue.Date;
                    }
                }
            }
            else if (value is int intValue)
            {
                if (otherValue is int intOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return intValue >= intOtherValue;
                    }
                    else
                    {
                        return intValue > intOtherValue;
                    }
                }
            }
            else if (value is uint uintValue)
            {
                if (otherValue is uint uintOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return uintValue >= uintOtherValue;
                    }
                    else
                    {
                        return uintValue > uintOtherValue;
                    }
                }
            }
            else if (value is long longValue)
            {
                if (otherValue is long longOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return longValue >= longOtherValue;
                    }
                    else
                    {
                        return longValue > longOtherValue;
                    }
                }
            }
            else if (value is ulong ulongValue)
            {
                if (otherValue is ulong ulongOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return ulongValue >= ulongOtherValue;
                    }
                    else
                    {
                        return ulongValue > ulongOtherValue;
                    }
                }
            }
            else if (value is byte byteValue)
            {
                if (otherValue is byte byteOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return byteValue >= byteOtherValue;
                    }
                    else
                    {
                        return byteValue > byteOtherValue;
                    }
                }
            }
            else if (value is short shortValue)
            {
                if (otherValue is short shortOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return shortValue >= shortOtherValue;
                    }
                    else
                    {
                        return shortValue > shortOtherValue;
                    }
                }
            }
            else if (value is ushort ushortValue)
            {
                if (otherValue is ushort ushortOtherValue)
                {
                    if (ruleMetadata.EqualValuesAreValid)
                    {
                        return ushortValue >= ushortOtherValue;
                    }
                    else
                    {
                        return ushortValue > ushortOtherValue;
                    }
                }
            }

            throw new InvalidOperationException($"Entity type {entityType} property {propName} " +
                $"value type {value.GetType().FullName} is not implemented for rule {nameof(GreaterThanRuleBase)}.");
        }
    }
}

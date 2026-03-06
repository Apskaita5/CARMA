using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;
using System;

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
            string otherPropName, IEqualToRuleMetadata ruleMetadata)
        {
            if (null == value && null == otherValue) return true;
            if (null == value || null == otherValue) return false;

            if (value.GetType() != otherValue.GetType()) throw new InvalidOperationException(
                $"Property value types on entity {entityType.FullName} are not the same: {propName}:" +
                $" {value.GetType().FullName} vs. {otherPropName}: {otherValue.GetType().FullName}.");

            if (value is double doubleValue)
            {
                if (otherValue is double doubleOtherValue)
                {
                    return doubleValue.EqualsTo(doubleOtherValue, ruleMetadata.SignificantDigits);
                }
            }
            else if (value is float floatValue)
            {
                if (otherValue is float floatOtherValue)
                {
                    return !(System.Math.Abs(floatValue - floatOtherValue) > float.Epsilon);
                }
            }
            else if (value is decimal decimalValue)
            {
                if (otherValue is decimal decimalOtherValue)
                {
                    return decimalValue.AccountingRound(ruleMetadata.SignificantDigits)
                        == decimalOtherValue.AccountingRound(ruleMetadata.SignificantDigits);
                }
            }
            else if (value is int intValue)
            {
                if (otherValue is int intOtherValue)
                {
                    return intValue == intOtherValue;
                }
            }
            else if (value is uint uintValue)
            {
                if (otherValue is uint uintOtherValue)
                {
                    return uintValue == uintOtherValue;
                }
            }
            else if (value is long longValue)
            {
                if (otherValue is long longOtherValue)
                {
                    return longValue == longOtherValue;
                }
            }
            else if (value is ulong ulongValue)
            {
                if (otherValue is ulong ulongOtherValue)
                {
                    return ulongValue == ulongOtherValue;
                }
            }
            else if (value is byte byteValue)
            {
                if (otherValue is byte byteOtherValue)
                {
                    return byteValue == byteOtherValue;
                }
            }
            else if (value is short shortValue)
            {
                if (otherValue is short shortOtherValue)
                {
                    return shortValue == shortOtherValue;
                }
            }
            else if (value is ushort ushortValue)
            {
                if (otherValue is ushort ushortOtherValue)
                {
                    return ushortValue == ushortOtherValue;
                }
            }
            else if (value is string stringValue)
            {
                if (otherValue is string stringOtherValue)
                {
                    return stringValue.Trim().Equals(stringOtherValue.Trim(), ruleMetadata.StringComparison);
                }
            }
            else if (value is IDomainEntityReference reference)
            {
                if (otherValue is IDomainEntityReference otherReference)
                {
                    return reference.ReferenceEqualsTo(otherReference);
                }
            }

            throw new InvalidOperationException($"Entity type {entityType.FullName} property {propName} value type " +
                $"{value.GetType().FullName} is not implemented.");
        }
    }
}

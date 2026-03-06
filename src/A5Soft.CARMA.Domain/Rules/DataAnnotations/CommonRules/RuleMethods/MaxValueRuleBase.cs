using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Globalization;

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
            if (null == value) return null;

            var valueType = value.GetType();
            var isIntType = valueType == typeof(int) || valueType == typeof(uint) ||
                valueType == typeof(byte) || valueType == typeof(short) || valueType == typeof(ushort) ||
                valueType == typeof(long) || valueType == typeof(ulong) || valueType == typeof(int?) ||
                valueType == typeof(uint?) || valueType == typeof(byte?) || valueType == typeof(short?) ||
                valueType == typeof(ushort?) || valueType == typeof(long?) || valueType == typeof(ulong?);
            if (isIntType != ruleMetadata.ExpectedIntValue) throw new InvalidOperationException(
                $"Value type mismatch on entity type " +
                $"{entityMetadata.EntityType.FullName} property {propertyMetadata.Name}.");

            bool isValid = true;
            if (value is int intValue)
            {
                isValid = intValue <= ruleMetadata.IntValueLimit;
            }
            else if (value is uint uintValue)
            {
                isValid = uintValue <= ruleMetadata.IntValueLimit;
            }
            else if (value is byte byteValue)
            {
                isValid = byteValue <= ruleMetadata.IntValueLimit;
            }
            else if (value is short shortValue)
            {
                isValid = shortValue <= ruleMetadata.IntValueLimit;
            }
            else if (value is ushort ushortValue)
            {
                isValid = ushortValue <= ruleMetadata.IntValueLimit;
            }
            else if (value is long longValue)
            {
                isValid = longValue <= ruleMetadata.IntValueLimit;
            }
            else if (value is ulong ulongValue)
            {
                isValid = ulongValue <= Convert.ToUInt64(ruleMetadata.IntValueLimit);
            }
            else if (value is double dblValue)
            {
                isValid = ruleMetadata.DblValueLimit.GreaterThan(dblValue, ruleMetadata.SignificantDigits) ||
                    ruleMetadata.DblValueLimit.EqualsTo(dblValue, ruleMetadata.SignificantDigits);
            }
            else if (value is float floatValue)
            {
                isValid = ruleMetadata.DblValueLimit.GreaterThan(floatValue, ruleMetadata.SignificantDigits) ||
                    ruleMetadata.DblValueLimit.EqualsTo(floatValue, ruleMetadata.SignificantDigits);
            }
            else if (value is decimal decimalValue)
            {
                var threshold = decimal.Parse(ruleMetadata.DblValueLimit.ToString(CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture).AccountingRound(ruleMetadata.SignificantDigits);
                isValid = decimalValue.AccountingRound(ruleMetadata.SignificantDigits) <= threshold;
            }
            else throw new InvalidOperationException($"Unexpected property value type {value.GetType().FullName} " +
                $"for entity type {entityMetadata.EntityType.FullName} property {propertyMetadata.Name}.");

            if (isValid) return null;

            return GetLocalizedErrorMessageFor(propertyMetadata.GetDisplayName(),
                isIntType ? ruleMetadata.IntValueLimit.ToString() : ruleMetadata.DblValueLimit.ToString());
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName, string maxValue);
    }
}

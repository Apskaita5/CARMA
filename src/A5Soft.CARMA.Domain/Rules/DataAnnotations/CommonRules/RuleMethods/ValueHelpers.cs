using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Orm;
using System;
using System.Collections;
using System.Globalization;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    internal static class ValueHelpers
    {
        /// <summary>
        /// Gets a value indicationg whether <paramref name="value"/> is a valid property value.
        /// Valid property values are those that are not considered "empty", e.g. 0.
        /// </summary>
        /// <param name="value">a value to check</param>
        /// <param name="isKeyReference">whether the value is expected to be a key reference</param>
        /// <param name="allowNegative">whether negative numeric values are considered valid</param>
        /// <param name="significantDigits">significant digits for numeric value comparission</param>
        /// <returns>a value indicationg whether <paramref name="value"/> is a valid property value</returns>
        public static bool IsValidValue(object value, bool isKeyReference, bool allowNegative,
            int significantDigits, bool allowEmptyStrings)
        {
            if (null == value || value == DBNull.Value) return false; // handles nullable types that does not have a value as well

            bool isValid = true;
            if (isKeyReference)
            {
                if (value is string val) isValid = val.IsValidKey();
                else if (value is int intVal) isValid = intVal.IsValidKey();
                else if (value is long longVal) isValid = longVal.IsValidKey();
                else if (value is Guid guidVal) isValid = guidVal.IsValidKey();
                else throw new NotSupportedException(
                    $"Reference key type {value.GetType().FullName} is not supported by {nameof(ValueHelpers)}.");
            }
            else if (value is string val)
            {
                isValid = allowEmptyStrings || !val.IsNullOrWhiteSpace();
            }
            else if (value is decimal decimalValue)
            {
                if (allowNegative) isValid = decimalValue.AccountingRound(significantDigits) != 0.0m;
                else isValid = decimalValue.AccountingRound(significantDigits) > 0.0m;
            }
            else if (value is double doubleValue)
            {
                if (allowNegative) isValid = !doubleValue.EqualsTo(0.0, significantDigits);
                else isValid = doubleValue.GreaterThan(0.0, significantDigits);
            }
            else if (value is byte byteValue)
            {
                if (allowNegative) isValid = byteValue != 0;
                else isValid = byteValue > 0;
            }
            else if (value is short int16Value)
            {
                if (allowNegative) isValid = int16Value != 0;
                else isValid = int16Value > 0;
            }
            else if (value is int intValue)
            {
                if (allowNegative) isValid = intValue != 0;
                else isValid = intValue > 0;
            }
            else if (value is long longValue)
            {
                if (allowNegative) isValid = longValue != 0;
                else isValid = longValue > 0;
            }
            else if (value is float floatValue)
            {
                if (allowNegative) isValid = !((double)floatValue).EqualsTo(0.0, significantDigits);
                else isValid = ((double)floatValue).GreaterThan(0.0, significantDigits);
            }
            else if (value is sbyte sbyteValue) isValid = allowNegative ? sbyteValue != 0 : sbyteValue > 0;
            else if (value is ushort ushortValue) isValid = ushortValue > 0;
            else if (value is uint uintValue) isValid = uintValue > 0;
            else if (value is ulong ulongValue) isValid = ulongValue > 0;
            else if (value is DateTime dateValue) isValid = dateValue != default;
            else if (value is IList list)
            {
                isValid = list.Count > 0;
            }

            return isValid;
        }

        /// <summary>
        /// Compares a numeric property value with a limit using the value's own type.
        /// Returns &lt;0 if value is less than limit, 0 if equal, &gt;0 if greater.
        /// </summary>
        public static int CompareToLimit(object value, double limit, int digits)
        {
            var valueToCompare = value ?? 0;

            switch (valueToCompare)
            {
                case int _:
                case long _:
                case short _:
                case sbyte _:
                case uint _:
                case ulong _:
                case ushort _:
                case byte _:
                    // integers: exact compare via decimal (no rounding)
                    return Convert.ToDecimal(valueToCompare).CompareTo(ToDecimal(limit));

                case decimal d:
                    return d.AccountingRound(digits).CompareTo(ToDecimal(limit).AccountingRound(digits));

                case double dbl:
                    return dbl.EqualsTo(limit, digits) ? 0 : dbl.CompareTo(limit);

                case float f:
                    return ((double)f).EqualsTo(limit, digits) ? 0 : ((double)f).CompareTo(limit);

                default:
                    throw new InvalidOperationException(
                        $"Value range rule is not applicable for value of type {(value?.GetType().FullName ?? "[unknown, null value]")}.");
            }
        }

        /// <summary>
        /// Formats a value limit for display in an error message, using the property type
        /// to choose the representation, i.e. the way the limit is actually compared.
        /// </summary>
        /// <param name="propertyType">type of the validated property (nullable types are supported)</param>
        /// <param name="limit">limit value as defined in the rule metadata</param>
        /// <param name="digits">significant digits used for non-integer comparison</param>
        /// <param name="isMin">whether the limit is a lower bound (otherwise - upper bound).
        /// Determines the effective integer bound for a fractional limit.</param>
        /// <param name="formatProvider">format provider to use; current culture if null</param>
        /// <returns>a formatted limit value</returns>
        public static string FormatLimit(Type propertyType, double limit, int digits, bool isMin,
            IFormatProvider formatProvider = null)
        {
            if (null == propertyType) throw new ArgumentNullException(nameof(propertyType));

            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var culture = formatProvider ?? CultureInfo.CurrentCulture;
            var decimalLimit = ToDecimal(limit);

            if (IsIntegral(type))
            {
                // effective bound for integers: MinValue(0.5) => 1, MaxValue(10.5) => 10
                var effective = isMin ? System.Math.Ceiling(decimalLimit) : System.Math.Floor(decimalLimit);
                return effective.ToString("#,##0", culture);
            }

            // decimal, double, float: show the limit rounded exactly as it is compared
            return decimalLimit.AccountingRound(digits).ToString(GetDecimalFormat(digits), culture);
        }

        /// <summary>
        /// Gets normalized operands for a reference property rule.
        /// </summary>
        /// <returns>false if the rule is not applicable (null value while <see cref="IRuleMetadata.NullIsValid"/>
        /// is true), true otherwise</returns>
        /// <exception cref="InvalidOperationException">property types do not match, or a date value is null
        /// while <see cref="IRuleMetadata.NullIsValid"/> is false</exception>
        public static bool TryGetOperands(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IRefPropertyRuleMetadata ruleMetadata, out RefOperands operands)
        {
            operands = default;

            var refProp = ruleMetadata.GetReferenceProperty(entityMetadata);
            var type = GetCommonType(propertyMetadata, refProp, entityMetadata);

            if (value == DBNull.Value) value = null;
            var refValue = refProp.GetValue(instance);
            if (refValue == DBNull.Value) refValue = null;

            if (null == value || null == refValue)
            {
                if (ruleMetadata.NullIsValid) return false;
                value = NormalizeNull(value, type, propertyMetadata.Name, entityMetadata);
                refValue = NormalizeNull(refValue, type, refProp.Name, entityMetadata);
            }

            operands = new RefOperands(type, value, refValue, refProp);
            return true;
        }

        /// <summary>
        /// Compares operands of a numeric or date type.
        /// </summary>
        /// <returns>&lt;0 if value is less than reference, 0 if equal, &gt;0 if greater</returns>
        public static int CompareValues(RefOperands operands, int digits, bool evaluateTime)
        {
            var type = operands.ValueType;
            var a = operands.Value;
            var b = operands.ReferenceValue;

            if (type == typeof(decimal))
                return ((decimal)a).AccountingRound(digits).CompareTo(((decimal)b).AccountingRound(digits));

            if (type == typeof(double) || type == typeof(float))
            {
                var x = Convert.ToDouble(a, CultureInfo.InvariantCulture);
                var y = Convert.ToDouble(b, CultureInfo.InvariantCulture);
                return x.EqualsTo(y, digits) ? 0 : x.CompareTo(y);
            }

            if (type == typeof(DateTime))
                return evaluateTime
                    ? ((DateTime)a).CompareTo((DateTime)b)
                    : ((DateTime)a).Date.CompareTo(((DateTime)b).Date);

            if (type == typeof(DateTimeOffset))
                return evaluateTime
                    ? ((DateTimeOffset)a).CompareTo((DateTimeOffset)b)
                    : ((DateTimeOffset)a).Date.CompareTo(((DateTimeOffset)b).Date);

            if (IsIntegral(type))
                return ((IComparable)a).CompareTo(b);   // same boxed type guaranteed

            throw new InvalidOperationException($"Values of type {type.FullName} cannot be ordered.");
        }

        /// <summary>
        /// Gets a value indicating whether operands are equal. Numbers are rounded to digits
        /// (if not integer), dates are compared exactly, strings using the comparison specified,
        /// other types using <see cref="object.Equals(object, object)"/>.
        /// </summary>
        public static bool ValuesEqual(RefOperands operands, int digits, StringComparison stringComparison)
        {
            var type = operands.ValueType;

            if (type == typeof(string))
                return string.Equals((string)operands.Value, (string)operands.ReferenceValue, stringComparison);

            if (IsNumeric(type) || IsDate(type))
                return CompareValues(operands, digits, true) == 0;

            return Equals(operands.Value, operands.ReferenceValue);
        }


        private static bool IsIntegral(Type type)
            => type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(sbyte)
            || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(byte);

        private static bool IsNumeric(Type type)
        => IsIntegral(type) || type == typeof(decimal) || type == typeof(double) || type == typeof(float);

        private static bool IsDate(Type type)
            => type == typeof(DateTime) || type == typeof(DateTimeOffset);

        private static Type GetUnderlyingType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

        private static string GetDecimalFormat(int digits)
            => digits > 0 ? "#,##0." + new string('#', digits) : "#,##0";

        private static decimal ToDecimal(double limit)
        {
            if (limit >= (double)decimal.MaxValue) return decimal.MaxValue;
            if (limit <= (double)decimal.MinValue) return decimal.MinValue;
            return (decimal)limit;
        }

        private static IPropertyMetadata GetReferenceProperty(this IRefPropertyRuleMetadata ruleMetadata,
            IEntityMetadata entityMetadata)
        {
            return entityMetadata.GetPropertyMetadata(ruleMetadata.ReferenceProperty)
                ?? throw new InvalidOperationException($"No metadata for reference property " +
                    $"{ruleMetadata.ReferenceProperty} on type {entityMetadata.EntityType.FullName}.");
        }

        private static Type GetCommonType(IPropertyMetadata property, IPropertyMetadata refProperty,
        IEntityMetadata entityMetadata)
        {
            var type = GetUnderlyingType(property.PropertyType);
            var refType = GetUnderlyingType(refProperty.PropertyType);
            if (type != refType) throw new InvalidOperationException(
                $"Property {entityMetadata.EntityType.Name}.{property.Name} ({property.PropertyType.Name}) " +
                $"and reference property {refProperty.Name} ({refProperty.PropertyType.Name}) " +
                $"must be of the same type.");
            return type;
        }

        private static object NormalizeNull(object value, Type type, string propertyName,
            IEntityMetadata entityMetadata)
        {
            if (null != value) return value;

            if (IsNumeric(type)) return Activator.CreateInstance(type);   // 0 of the exact type

            if (IsDate(type)) throw new InvalidOperationException(
                $"Property {entityMetadata.EntityType.Name}.{propertyName} value is null; " +
                $"date comparison requires a value when null values are not valid.");

            if (type == typeof(string)) return string.Empty;

            return null;   // other types: null only equals null
        }
    }
}

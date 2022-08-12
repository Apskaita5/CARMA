using System;
using System.Collections.Generic;
using System.Globalization;
using A5Soft.CARMA.Domain.Math;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for minimal numeric value validation.
    /// </summary> 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class MinValueAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(float),
            typeof(double), typeof(decimal), typeof(byte), typeof(short), typeof(ushort), typeof(int),
            typeof(uint), typeof(long), typeof(ulong), typeof(float?), typeof(double?),
            typeof(decimal?), typeof(byte?), typeof(short?), typeof(ushort?), typeof(int?),
            typeof(uint?), typeof(long?), typeof(ulong?) };


        /// <summary>
        /// min decimal (double, float) value validation
        /// </summary>
        /// <param name="minValue">min allowed value</param>
        /// <param name="digits">significant digits to compare double or decimal values (default - 2)</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected MinValueAttributeBase(double minValue, int digits = 2, RuleSeverity severity = RuleSeverity.Error)
            : base()
        {
            Digits = digits;
            Severity = severity;
            MinDblValue = minValue;
            ExpectedIntValue = false;
        }

        /// <summary>
        /// min int value validation
        /// </summary>
        /// <param name="minValue">min allowed value</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected MinValueAttributeBase(int minValue, RuleSeverity severity = RuleSeverity.Error)
            : base()
        {
            Digits = 2;
            Severity = severity;
            MinIntValue = minValue;
            ExpectedIntValue = true;
        }


        /// <summary>
        /// Gets a value indicating wheteher an integer value is expected (otherwise decimal, double or float).
        /// </summary>
        public bool ExpectedIntValue { get; }

        /// <summary>
        /// Gets a min allowed integer value.
        /// </summary>
        public int MinIntValue { get; }

        /// <summary>
        /// Gets a min allowed double (decimal, float) value.
        /// </summary>
        public double MinDblValue { get; }

        /// <summary>
        /// Gets significant digits to compare double or decimal values.
        /// </summary>
        public int Digits { get; }


        /// <inheritdoc/>
        protected override bool NullIsAlwaysValid => true;

        /// <inheritdoc/>
        protected override Type[] SupportedValueTypes => _supportedValueTypes;

        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var valueType = value.GetType();
            var isIntType = typeof(decimal) != valueType && typeof(double) != valueType
                && typeof(float) != valueType && typeof(decimal?) != valueType
                && typeof(double?) != valueType && typeof(float?) != valueType;
            if (isIntType != ExpectedIntValue) throw new InvalidOperationException(
                $"Value type mismatch for rule {this.GetType().FullName} on entity type {entityType.FullName} property {propertyDisplayName}.");

            bool isValid = true;
            if (value is int intValue)
            {
                isValid = intValue >= MinIntValue;
            }
            else if (value is uint uintValue)
            {
                isValid = uintValue >= MinIntValue;
            }
            else if (value is byte byteValue)
            {
                isValid = byteValue >= MinIntValue;
            }
            else if (value is short shortValue)
            {
                isValid = shortValue >= MinIntValue;
            }
            else if (value is ushort ushortValue)
            {
                isValid = ushortValue >= MinIntValue;
            }
            else if (value is long longValue)
            {
                isValid = longValue >= MinIntValue;
            }
            else if (value is ulong ulongValue)
            {
                isValid = ulongValue >= Convert.ToUInt64(MinIntValue);
            }
            else if (value is double dblValue)
            {
                isValid = dblValue.GreaterThan(MinDblValue, Digits) || MinDblValue.EqualsTo(dblValue, Digits);
            }
            else if (value is float floatValue)
            {
                isValid = Convert.ToDouble(floatValue).GreaterThan(MinDblValue, Digits) || MinDblValue.EqualsTo(floatValue, Digits);
            }
            else if (value is decimal decimalValue)
            {
                var threshold = decimal.Parse(MinDblValue.ToString(CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture).AccountingRound(Digits);
                isValid = decimalValue.AccountingRound(Digits) >= threshold;
            }
            else throw new InvalidOperationException($"Unexpected property value type {value.GetType().FullName} " +
                $"in rule {this.GetType().FullName} for entity type {entityType.FullName} property {propertyDisplayName}.");

            if (isValid) return null;

            return GetLocalizedErrorMessageFor(propertyDisplayName);
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName);
    }
}

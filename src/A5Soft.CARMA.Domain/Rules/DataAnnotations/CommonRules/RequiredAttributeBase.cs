using A5Soft.CARMA.Domain.Math;
using System;
using System.Collections;
using A5Soft.CARMA.Domain.Orm;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for required rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class RequiredAttributeBase : PropertyRuleAttributeBase
    {
        /// <inheritdoc />
        protected RequiredAttributeBase() : base() {}


        /// <summary>
        /// Gets or sets a value indicating whether the property value is a reference (primary key) of a domain entity.
        /// </summary>
        public bool IsKeyReference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an empty string is valid.
        /// Only applicable for string properties. Default is false.
        /// </summary>
        public bool AllowEmptyStrings { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether negative numeric property value is valid.
        /// Only applicable for numeric properties. Default is false.
        /// </summary>
        public bool AllowNegative { get; set; } = false;

        /// <summary>
        /// Gets or sets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        public int SignificantDigits { get; set; } = 2;


        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            if (null == value) return GetLocalizedErrorMessageFor(propertyDisplayName); // handles nullable types that does not have a value as well

            bool isValid = true;
            if (IsKeyReference)
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
                if (AllowNegative) isValid = decimalValue.AccountingRound(SignificantDigits) != 0.0m;
                else isValid = decimalValue.AccountingRound(SignificantDigits) > 0.0m;
            }
            else if (value is double doubleValue)
            {
                if (AllowNegative) isValid = !doubleValue.EqualsTo(0.0, SignificantDigits);
                else isValid = doubleValue.GreaterThan(0.0, SignificantDigits);
            }
            else if (value is byte byteValue)
            {
                if (AllowNegative) isValid = byteValue != 0;
                else isValid = byteValue > 0;
            }
            else if (value is short int16Value)
            {
                if (AllowNegative) isValid = int16Value != 0;
                else isValid = int16Value > 0;
            }
            else if (value is int intValue)
            {
                if (AllowNegative) isValid = intValue != 0;
                else isValid = intValue > 0;
            }
            else if (value is long longValue)
            {
                if (AllowNegative) isValid = longValue != 0;
                else isValid = longValue > 0;
            }
            else if (value is IList list)
            {
                isValid = list.Count > 0;
            }

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

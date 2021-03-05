using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for required rule.
    /// Can use it as is by setting ErrorMessageResourceType and ErrorMessageResourceName values
    /// in the attribute decorator or inherit this class and set ErrorMessageResourceType and ErrorMessageResourceName
    /// in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute,
        IPropertyValidationAttribute
    {
        
        /// <summary>
        /// Gets or sets a value indicating whether the property value is a reference (primary key) of a domain entity.
        /// </summary>
        public bool IsKeyReference { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; set; }


        /// <summary>
        /// Extended and localized version of Required that handles:
        /// - string (IsNullOrWhiteSpace);
        /// - key references of types int, long, Guid and string;
        /// - nullable types (shall have value to be valid);
        /// - IList (shall have at least one item to be valid);
        /// - numeric types (based on AllowNegative and SignificantDigits values);
        /// - any other type (null != value, which includes ILookup implementations).
        /// </summary>
        public RequiredAttribute()
        {
            this.AllowEmptyStrings = false;
        }


        /// <inheritdoc cref="System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid" />
        public override bool IsValid(object value)
        {
            if (Severity == RuleSeverity.Error) return IsValidInternal(value);
            return true;
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (IsValidInternal(propInfo.GetValue(instance))) return null;

            return new BrokenRule(this.GetType().FullName, propInfo.Name, string.Format(
                CultureInfo.CurrentCulture, this.ErrorMessageString, propInfo.GetDisplayName()), Severity);
        }


        private bool IsValidInternal(object value)
        {
            if (null == value) return false;  // handles nullable types that does not have a value as well

            if (IsKeyReference)
            {
                if (value is string val)
                {
                    return val.IsValidKey();
                }
                else if (value is int intVal)
                {
                    return intVal.IsValidKey();
                }
                else if (value is long longVal)
                {
                    return longVal.IsValidKey();
                }
                else if (value is Guid guidVal)
                {
                    return guidVal.IsValidKey();
                }
                
                throw new NotSupportedException(
                    $"Key type {value.GetType().FullName} is not supported by RequiredAttribute.");
            }
            else if (value is string val)
            {
                return !val.IsNullOrWhiteSpace();
            }
            else if (value is decimal decimalValue)
            {
                if (AllowNegative) return decimalValue.AccountingRound(SignificantDigits) != 0.0m;
                return decimalValue.AccountingRound(SignificantDigits) > 0.0m;
            }
            else if (value is double doubleValue)
            {
                if (AllowNegative) return !doubleValue.EqualsTo(0.0, SignificantDigits);
                return doubleValue.GreaterThan(0.0, SignificantDigits);
            }
            else if (value is byte byteValue)
            {
                if (AllowNegative) return byteValue != 0;
                return byteValue > 0;
            }
            else if (value is Int16 int16Value)
            {
                if (AllowNegative) return int16Value != 0;
                return int16Value > 0;
            }
            else if (value is int intValue)
            {
                if (AllowNegative) return intValue != 0;
                return intValue > 0;
            }
            else if (value is long longValue)
            {
                if (AllowNegative) return longValue != 0;
                return longValue > 0;
            }
            else if (value is IList list)
            {
                return (null != list && list.Count > 0);
            }

            return true;
        }

    }
}

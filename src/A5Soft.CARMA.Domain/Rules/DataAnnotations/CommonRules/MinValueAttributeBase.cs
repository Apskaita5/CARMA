using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for minimal numeric value validation.
    /// </summary> 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class MinValueAttributeBase : RangeAttribute, IPropertyValidationAttribute
    {
        /// <summary>
        /// Gets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; }

        /// <summary>
        /// Gets significant digits to compare double or decimal values (default - 2).
        /// </summary>
        public int Digits { get; } 

        /// <inheritdoc />
        public override bool RequiresValidationContext
            => true;


        /// <summary>
        /// minimal numeric value validation
        /// </summary>
        /// <param name="minValue">minimal allowed value</param>
        /// <param name="digits">significant digits to compare double or decimal values (default - 2)</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected MinValueAttributeBase(double minValue, int digits = 2, RuleSeverity severity = RuleSeverity.Error)
            : base(minValue, double.MaxValue)
        {
            Digits = digits;
            Severity = severity;
        }

        /// <summary>
        /// minimal numeric value validation
        /// </summary>
        /// <param name="minValue">minimal allowed value</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected MinValueAttributeBase(int minValue, RuleSeverity severity = RuleSeverity.Error)
            : base(minValue, int.MaxValue)
        {
            Digits = 2;
            Severity = severity;
        }


        /// <inheritdoc />
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

            return new BrokenRule(this.GetType().FullName, propInfo.Name,
                GetLocalizedErrorMessageFor(propInfo.GetDisplayName()), Severity);
        }


        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (null == validationContext) throw new ArgumentNullException(nameof(validationContext));

            if (Severity != RuleSeverity.Error || IsValidInternal(value)) return null;

            var displayName = validationContext.DisplayName.IsNullOrWhiteSpace()
                ? validationContext.GetPropertyDisplayName()
                : validationContext.DisplayName;

            return new ValidationResult(GetLocalizedErrorMessageFor(displayName));
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName);

        private bool IsValidInternal(object value)
        {
            if (null == value) return true;

            if (value is int intValue)
            {
                return intValue >= (int)this.Minimum;
            }
            if (value is long longValue)
            {
                return longValue >= (int)this.Minimum;
            }
            if (value is double dblValue)
            {
                return dblValue.GreaterThan((double)this.Minimum, Digits) 
                    || dblValue.EqualsTo((double)this.Minimum, Digits);
            }
            if (value is decimal decimalValue)
            {
                var threshold = decimal.Parse(((double)Minimum).ToString(CultureInfo.InvariantCulture), 
                    CultureInfo.InvariantCulture).AccountingRound(Digits);
                return decimalValue.AccountingRound(Digits) >= threshold;
            }
            else
            {
                throw new NotSupportedException(
                    $"Value type {value.GetType().FullName} is not supported by RangeAttribute.");
            }
        }

    }
}

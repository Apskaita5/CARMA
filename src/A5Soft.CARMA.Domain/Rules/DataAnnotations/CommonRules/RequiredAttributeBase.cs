using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for required rule.
    /// </summary> 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class RequiredAttributeBase : RequiredAttribute, IPropertyValidationAttribute
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

        /// <inheritdoc />
        public override bool RequiresValidationContext 
            => true;


        /// <inheritdoc />
        protected RequiredAttributeBase()
        {
            this.AllowEmptyStrings = false;
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
            if (null == value) return false;  // handles nullable types that does not have a value as well

            if (IsKeyReference)
            {
                if (value is string val) return val.IsValidKey();
                if (value is int intVal) return intVal.IsValidKey();
                if (value is long longVal) return longVal.IsValidKey();
                if (value is Guid guidVal) return guidVal.IsValidKey();
                if (value is IDomainEntityIdentity identity) return !identity.IsNew;
                throw new NotSupportedException(
                    $"Reference key type {value.GetType().FullName} is not supported by RequiredAttribute.");
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
            else if (value is short int16Value)
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
                return (list.Count > 0);
            }
            else if (value is IDomainEntityIdentity identity)
            {
                return !identity.IsNew;
            }
            else if (value is ILookup lookup)
            {
                return (null != lookup.Id && !lookup.Id.IsNew);
            }

            return true;
        }

    }
}

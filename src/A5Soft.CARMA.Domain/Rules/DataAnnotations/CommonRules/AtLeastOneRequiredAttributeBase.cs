using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Orm;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for at least one property required rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class AtLeastOneRequiredAttributeBase : ValidationAttribute,
        IPropertyValidationAttribute, IDependsOnProperties
    {
        /// <summary>
        /// Gets a name of the other property to check.
        /// </summary>
        public string ReferenceProperty { get; }

        /// <inheritdoc cref="IDependsOnProperties.DependsOnProperties" />
        public List<string> DependsOnProperties
            => new List<string>(new[] { ReferenceProperty });

        /// <summary>
        /// Gets or sets a value indicating whether the property value is a reference (primary key) of a domain entity.
        /// </summary>
        public bool IsKeyReference { get; set; } = false;

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
        public RuleSeverity Severity { get; set; } = RuleSeverity.Error;

        /// <inheritdoc />
        public override bool RequiresValidationContext
            => true;


        /// <summary>
        /// Creates a new instance of AtLeastOneRequiredAttribute.
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        protected AtLeastOneRequiredAttributeBase(string referenceProperty)
            : base()
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty;
        }

                               
        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            var secondPropInfo = relatedProps.FirstOrDefault(
                p => p.Name == ReferenceProperty);
            if (null == secondPropInfo) throw new InvalidOperationException(
                $"No such property {ReferenceProperty} on type {instance.GetType().FullName}.");

            if (IsValueValid(propInfo.GetValue(instance)) || IsValueValid(secondPropInfo.GetValue(instance))) return null;

            return new BrokenRule(this.GetType().FullName, propInfo.Name,
                GetLocalizedErrorMessageFor(propInfo.GetDisplayName(),
                    secondPropInfo.GetDisplayName()), Severity);
        }


        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (Severity != RuleSeverity.Error) return null;

            if (validationContext.ObjectInstance.IsNull()) throw new InvalidOperationException(
                "Object instance is null in validation context.");

            var otherPropInfo = validationContext.ObjectType.GetProperty(ReferenceProperty);
            if (null == otherPropInfo) throw new InvalidOperationException(
                $"Property {ReferenceProperty} does not exist on type {validationContext.ObjectType.FullName}.");

            var otherValue = otherPropInfo.GetValue(validationContext.ObjectInstance);

            if (IsValueValid(value) || IsValueValid(otherValue)) return null;

            var firstPropDisplayName = validationContext.DisplayName.IsNullOrWhiteSpace()
                ? validationContext.GetPropertyDisplayName()
                : validationContext.DisplayName;

            var secondPropDisplayName = validationContext.GetPropertyDisplayName(ReferenceProperty);

            return new ValidationResult(GetLocalizedErrorMessageFor(
                firstPropDisplayName, secondPropDisplayName));
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <param name="localizedOtherPropName">a localized name of the other property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName, string localizedOtherPropName);

        private bool IsValueValid(object value)
        {
            if (null == value) return false;  // handles nullable types that does not have a value as well

            if (IsKeyReference)
            {
                if (value is string val) return val.IsValidKey();
                if (value is int intVal) return intVal.IsValidKey();
                if (value is long longVal) return longVal.IsValidKey();
                if (value is Guid guidVal) return guidVal.IsValidKey();
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

            return true;
        }
                 
    }
}

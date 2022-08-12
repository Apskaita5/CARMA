using System;
using System.Collections;
using System.Collections.Generic;
using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Orm;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for at least one property required rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class AtLeastOneRequiredAttributeBase : PropertyRuleAttributeBase
    {
        /// <summary>
        /// Creates a new instance of AtLeastOneRequired rule.
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        protected AtLeastOneRequiredAttributeBase(string referenceProperty)
            : base()
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty;
        }


        /// <summary>
        /// Gets a name of the other property to check.
        /// </summary>
        public string ReferenceProperty { get; }

        /// <inheritdoc />
        protected override List<string> DependsOnOtherProperties
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

        /// <inheritdoc />
        protected override bool EntityInstanceRequired => true;


        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var otherProp = otherProperties[ReferenceProperty];

            if (IsValueValid(value) || IsValueValid(otherProp.Value)) return null;

            return GetLocalizedErrorMessageFor(propertyDisplayName, otherProp.DisplayName);
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
            if (null == value) return false; // handles nullable types that does not have a value as well

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

            return isValid;
        }
    }
}

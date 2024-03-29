﻿using System;
using System.Collections.Generic;
using A5Soft.CARMA.Domain.Math;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for (property) values equal rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class EqualToAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(string), typeof(bool),
            typeof(float), typeof(double), typeof(decimal), typeof(byte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(bool?), typeof(float?),
            typeof(double?), typeof(decimal?), typeof(byte?), typeof(short?), typeof(ushort?),
            typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(IDomainEntityReference) };


        /// <summary>
        /// (property) values equal rule
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        protected EqualToAttributeBase(string referenceProperty)
            : base()
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty;
        }


        /// <summary>
        /// Gets a name of the other property to check.
        /// </summary>
        public string ReferenceProperty { get; }

        /// <summary>
        /// Gets or sets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        public int SignificantDigits { get; set; } = 2;

        /// <summary>
        /// Gets or sets a string comparision method.
        /// </summary>
        public StringComparison StringComparison { get; set; } = StringComparison.CurrentCulture;

        /// <inheritdoc cref="IDependsOnProperties.DependsOnProperties" />
        protected override List<string> DependsOnOtherProperties
            => new List<string>(new[] { ReferenceProperty });

        /// <inheritdoc />
        protected override bool EntityInstanceRequired => true;

        /// <inheritdoc />
        protected override Type[] SupportedValueTypes => _supportedValueTypes;


        /// <inheritdoc />
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var secondProp = otherProperties[ReferenceProperty];

            if (IsValidInternal(value, secondProp.Value, entityType, propertyDisplayName)) return null;

            return GetLocalizedErrorMessageFor(propertyDisplayName, secondProp.DisplayName);
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <param name="localizedOtherPropName">a localized name of the other property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName, string localizedOtherPropName);

        private bool IsValidInternal(object value, object otherValue, Type entityType, string propName)
        {
            if (null == value && null == otherValue) return true;
            if (null == value || null == otherValue) return false;

            if (value.GetType() != otherValue.GetType()) throw new InvalidOperationException(
                $"Property value types on entity {entityType.FullName} are not the same: {value.GetType().FullName} vs. {otherValue.GetType().FullName}.");

            if (value is double doubleValue)
            {
                if (otherValue is double doubleOtherValue)
                {
                    return doubleValue.EqualsTo(doubleOtherValue, SignificantDigits);
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
                    return decimalValue.AccountingRound(SignificantDigits) == decimalOtherValue.AccountingRound(SignificantDigits);
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
                    return stringValue.Trim().Equals(stringOtherValue.Trim(), StringComparison);
                }
            }
            else if (value is IDomainEntityReference reference)
            {
                if (otherValue is IDomainEntityReference otherReference)
                {
                    return reference.ReferenceEqualsTo(otherReference);
                }
            }

            throw new InvalidOperationException($"Entity type {entityType} property {propName} value type " +
                $"{value.GetType().FullName} is not implemented for rule {nameof(GreaterThanAttributeBase)}.");
        }
    }
}

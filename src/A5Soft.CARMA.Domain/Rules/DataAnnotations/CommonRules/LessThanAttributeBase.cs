﻿using A5Soft.CARMA.Domain.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for (property) values less than rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class LessThanAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(DateTime), typeof(float),
            typeof(double), typeof(decimal), typeof(byte), typeof(short), typeof(ushort), typeof(int),
            typeof(uint), typeof(long), typeof(ulong), typeof(DateTime?), typeof(float?),
            typeof(double?), typeof(decimal?), typeof(byte?), typeof(short?), typeof(ushort?), typeof(int?),
            typeof(uint?), typeof(long?), typeof(ulong?) };


        /// <summary>
        /// Property value is greater than <see cref="ReferenceProperty"/> value rule.
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        /// <remarks>For <see langword="null"/> values rule is not applied, i.e. always returns valid result.</remarks>
        protected LessThanAttributeBase(string referenceProperty)
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
        /// Gets or sets a value indicating whether a time component of a <see cref="DateTime"/>
        /// values should be evaluated.
        /// Only applicable for <see cref="DateTime"/> properties. Default is false.
        /// </summary>
        public bool EvaluateTime { get; set; } = false;

        /// <summary>
        /// Gets or sets value indicating whether equal property values are valid as well.
        /// </summary>
        public bool EqualValuesAreValid { get; set; } = false;

        /// <inheritdoc />
        protected override List<string> DependsOnOtherProperties
            => new List<string>(new[] { ReferenceProperty });

        /// <inheritdoc />
        protected override bool EntityInstanceRequired => true;

        /// <inheritdoc />
        protected override bool NullIsAlwaysValid => true;

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
            // Do not compare null values.
            if (null == value || null == otherValue) return true;

            if (value.GetType() != otherValue.GetType()) throw new InvalidOperationException(
                $"Property value types on entity {entityType.FullName} are not the same: {value.GetType().FullName} vs. {otherValue.GetType().FullName}.");

            if (value is double doubleValue)
            {
                if (otherValue is double doubleOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return doubleOtherValue.GreaterThan(doubleValue, SignificantDigits) ||
                            doubleValue.EqualsTo(doubleOtherValue, SignificantDigits);
                    }
                    else
                    {
                        return doubleOtherValue.GreaterThan(doubleValue, SignificantDigits);
                    }
                }
            }
            else if (value is float floatValue)
            {
                if (otherValue is float floatOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return floatOtherValue - floatValue >= float.Epsilon;
                    }
                    else
                    {
                        return floatOtherValue - floatValue > float.Epsilon;
                    }
                }
            }
            else if (value is decimal decimalValue)
            {
                if (otherValue is decimal decimalOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return decimalValue.AccountingRound(SignificantDigits) <= decimalOtherValue.AccountingRound(SignificantDigits);
                    }
                    else
                    {
                        return decimalValue.AccountingRound(SignificantDigits) < decimalOtherValue.AccountingRound(SignificantDigits);
                    }
                }
            }
            else if (value is DateTime dateTimeValue)
            {
                if (otherValue is DateTime dateTimeOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        if (EvaluateTime) return dateTimeValue <= dateTimeOtherValue;
                        return dateTimeValue.Date <= dateTimeOtherValue.Date;
                    }
                    else
                    {
                        if (EvaluateTime) return dateTimeValue < dateTimeOtherValue;
                        return dateTimeValue.Date < dateTimeOtherValue.Date;
                    }
                }
            }
            else if (value is int intValue)
            {
                if (otherValue is int intOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return intValue <= intOtherValue;
                    }
                    else
                    {
                        return intValue < intOtherValue;
                    }
                }
            }
            else if (value is uint uintValue)
            {
                if (otherValue is uint uintOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return uintValue <= uintOtherValue;
                    }
                    else
                    {
                        return uintValue < uintOtherValue;
                    }
                }
            }
            else if (value is long longValue)
            {
                if (otherValue is long longOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return longValue <= longOtherValue;
                    }
                    else
                    {
                        return longValue < longOtherValue;
                    }
                }
            }
            else if (value is ulong ulongValue)
            {
                if (otherValue is ulong ulongOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return ulongValue <= ulongOtherValue;
                    }
                    else
                    {
                        return ulongValue < ulongOtherValue;
                    }
                }
            }
            else if (value is byte byteValue)
            {
                if (otherValue is byte byteOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return byteValue <= byteOtherValue;
                    }
                    else
                    {
                        return byteValue < byteOtherValue;
                    }
                }
            }
            else if (value is short shortValue)
            {
                if (otherValue is short shortOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return shortValue <= shortOtherValue;
                    }
                    else
                    {
                        return shortValue < shortOtherValue;
                    }
                }
            }
            else if (value is ushort ushortValue)
            {
                if (otherValue is ushort ushortOtherValue)
                {
                    if (EqualValuesAreValid)
                    {
                        return ushortValue <= ushortOtherValue;
                    }
                    else
                    {
                        return ushortValue < ushortOtherValue;
                    }
                }
            }

            throw new InvalidOperationException($"Entity type {entityType} property {propName} value type {value.GetType().FullName} is not implemented for rule {nameof(GreaterThanAttributeBase)}.");
        }
    }
}

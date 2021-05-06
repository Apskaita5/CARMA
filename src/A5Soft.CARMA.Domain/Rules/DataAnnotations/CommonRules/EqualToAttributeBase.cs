using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for (property) values equal rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class EqualToAttributeBase : ValidationAttribute,
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
        /// (property) values equal rule
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        protected EqualToAttributeBase(string referenceProperty)
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

            if (IsValidInternal(propInfo.GetValue(instance), secondPropInfo.GetValue(instance))) return null;
                              
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

            if (IsValidInternal(value, otherValue)) return null;

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

        private bool IsValidInternal(object value, object otherValue)
        {
            if (null == value && null == otherValue) return true;
            if (null == value || null == otherValue) return false;

            if (!value.GetType().IsAssignableFrom(otherValue.GetType()) &&
                !otherValue.GetType().IsAssignableFrom(value.GetType())) throw new InvalidOperationException(
                $"Property value types are not the same: {value.GetType().FullName} vs. {otherValue.GetType().FullName}.");

            if (value is string stringValue)
            {
                return stringValue.Trim().Equals((otherValue as string)?.Trim() ?? string.Empty, StringComparison.CurrentCulture);
            }
            if (value is double doubleValue)
            {
                if (otherValue is double doubleOtherValue)
                {
                    return doubleValue.EqualsTo(doubleOtherValue, SignificantDigits);
                }
            }
            if (value is decimal decimalValue)
            {
                if (otherValue is decimal decimalOtherValue)
                {
                    return decimalValue.AccountingRound(SignificantDigits) == decimalOtherValue.AccountingRound(SignificantDigits);
                }
            }
            if (value is IDomainEntityIdentity identity)
            {
                return identity.IsSameIdentityAs(otherValue as IDomainEntityIdentity);
            }
            if (value is ILookup lookup)
            {
                if (null == lookup.Id && null == (otherValue as ILookup)?.Id) return true;
                if (null == lookup.Id || null == (otherValue as ILookup)?.Id) return false;
                return (otherValue as ILookup).Id.IsSameIdentityAs(lookup.Id);
            }

            return value == otherValue;
        }

    }
}

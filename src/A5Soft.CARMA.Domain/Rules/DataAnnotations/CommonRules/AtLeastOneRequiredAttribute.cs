﻿using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for at least one property required rule.
    /// Can use it as is by setting ErrorMessageResourceType and ErrorMessageResourceName values
    /// in the attribute decorator or inherit this class and set ErrorMessageResourceType and ErrorMessageResourceName
    /// in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AtLeastOneRequiredAttribute : ValidationAttribute,
        IPropertyValidationAttribute, IDependsOnProperties
    {

        /// <summary>
        /// Creates a new instance of AtLeastOneRequiredAttribute.
        /// </summary>
        /// <param name="referenceProperty">a name of the other property</param>
        public AtLeastOneRequiredAttribute(string referenceProperty)
            : base()
        {
            if (referenceProperty.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceProperty));
            ReferenceProperty = referenceProperty;
        }


        /// <inheritdoc cref="ValidationAttribute.RequiresValidationContext" />
        public override bool RequiresValidationContext
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a name of the other property to check.
        /// </summary>
        public string ReferenceProperty { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the property value is a reference (primary key) of a domain entity.
        /// </summary>
        public bool IsKeyReference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reference property value is a reference (primary key)
        /// of a domain entity.
        /// </summary>
        public bool ReferencePropertyIsKeyReference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; set; } = RuleSeverity.Error;

        /// <inheritdoc cref="IDependsOnProperties.DependsOnProperties" />
        public List<string> DependsOnProperties 
            => new List<string>(new [] { ReferenceProperty });

        
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (Severity != RuleSeverity.Error) return null;

            if (validationContext.ObjectInstance.IsNull()) throw new InvalidOperationException(
                "Object instance is null in validation context.");

            if (IsValidInternal(value, validationContext.ObjectInstance)) return null;

            var firstPropDisplayName = validationContext.DisplayName;
            if (firstPropDisplayName.IsNullOrWhiteSpace())
                firstPropDisplayName =
                    GetPropDisplayName(validationContext.ObjectInstance, validationContext.MemberName);

            var secondPropDisplayName = GetPropDisplayName(validationContext.ObjectInstance, 
                ReferenceProperty);

            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, 
                ErrorMessageString, firstPropDisplayName, secondPropDisplayName));
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (IsValidInternal(propInfo.GetValue(instance), instance)) return null;

            var secondPropInfo = relatedProps.FirstOrDefault(
                p => p.Name == ReferenceProperty);
            var secondPropDisplayName = ReferenceProperty;
            if (!secondPropInfo.IsNull()) secondPropDisplayName = secondPropInfo.GetDisplayName();

            return new BrokenRule(this.GetType().FullName, propInfo.Name, string.Format(
                this.ErrorMessageString, propInfo.GetDisplayName(), secondPropDisplayName), Severity);
        }


        private bool IsValidInternal(object value, object instance)
        {
            var otherPropInfo = instance.GetType().GetProperty(ReferenceProperty);
            if (null == otherPropInfo) throw new InvalidOperationException(
                $"Property {ReferenceProperty} does not exist on type {instance.GetType().FullName}.");

            var otherValue = otherPropInfo.GetValue(instance);
            
            return IsValueValid(value, IsKeyReference) || IsValueValid(otherValue, ReferencePropertyIsKeyReference);
        }

        private static bool IsValueValid(object value, bool isKeyReference)
        {
            if (null == value) return false;  // handles nullable types that does not have a value as well

            if (isKeyReference)
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
                else throw new NotSupportedException(
                    $"Key type {value.GetType().FullName} is not supported by AtLeastOneRequiredAttribute.");
            }
            else if (value is string val)
            {
                return !val.IsNullOrWhiteSpace();
            }
            else if (value is IList list)
            {
                return (null != list && list.Count > 0);
            }

            return true;
        }

        private static string GetPropDisplayName(object instance, string propName)
        {
            var displayAttr = instance.GetType().GetProperty(propName)
                .GetCustomAttributeWithInheritance<DisplayAttribute>();
            if (displayAttr.IsNull()) return propName;
            return displayAttr.GetName();
        }
        
    }
}

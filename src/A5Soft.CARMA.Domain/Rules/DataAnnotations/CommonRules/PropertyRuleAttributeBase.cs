using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for property rules, i.e. for rules that are applied to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class PropertyRuleAttributeBase : ValidationAttribute, IPropertyValidationAttribute, IDependsOnProperties
    {
        /// <inheritdoc/>
        protected PropertyRuleAttributeBase() {}

        #region Properties

        /// <summary>
        /// Gets a value indicating severity of broken rule. Default - Error.
        /// </summary>
        public RuleSeverity Severity { get; protected set; } = RuleSeverity.Error;

        /// <summary>
        /// Gets or sets a name of the bool switch property that controls
        /// whether the rule is enabled for an entity.
        /// </summary>
        public string EnabledPropertyName { get; set; }

        /// <inheritdoc cref="IDependsOnProperties.DependsOnProperties" />
        public List<string> DependsOnProperties
        {
            get
            {
                var result = EnabledPropertyName.IsNullOrWhiteSpace() ? new List<string>()
                    : new List<string>(new[] { EnabledPropertyName });
                if (null != DependsOnOtherProperties && DependsOnOtherProperties.Count > 0)
                    result.AddRange(DependsOnOtherProperties);
                return result;
            }
        }

        /// <inheritdoc />
        public override bool RequiresValidationContext
            => true;

        /// <summary>
        /// Override the property if the validation rules requeries other property values.
        /// </summary>
        protected virtual List<string> DependsOnOtherProperties
            => new List<string>();

        /// <summary>
        /// Override the property to return types of the property values supported by the rule
        /// (if a limited set is supported). Return null or empty array if there are no limitations (default).
        /// </summary>
        protected virtual Type[] SupportedValueTypes { get; } = null;

        /// <summary>
        /// Override the property if you need an instance of the evaluated entity for the rule.
        /// Default - false.
        /// </summary>
        protected virtual bool EntityInstanceRequired { get; } = false;

        /// <summary>
        /// Override the property to return a type of the entity supported by the rule.
        /// Return null if there are no entity type limitations (default).
        /// </summary>
        /// <remarks>Only relevant if <see cref="EntityInstanceRequired"/>.</remarks>
        protected virtual Type SupportedEntityType { get; } = null;

        /// <summary>
        /// Override the property if you do not wish to evaluate null values.
        /// I.e. if null values are always valid. Default - false.
        /// </summary>
        protected virtual bool NullIsAlwaysValid { get; } = false;

        #endregion

        #region Methods

        /// <summary>
        /// Returns a broken rule if the business rule for the entity instance is broken.
        /// Otherwise - returns null.
        /// </summary>
        /// <param name="instance">an instance of the entity to check the rule for</param>
        /// <param name="propInfo">the metadata of the property to check the rule for</param>
        /// <param name="relatedProps">the metadata of the properties that are affected (if any)</param>
        /// <returns>a broken rule if the business rule for the entity instance is broken,
        /// otherwise - null.</returns>
        /// <remarks>Only override this method if you need to access relatedProps for
        /// validation or error description</remarks>
        public virtual BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (null == propInfo) throw new ArgumentNullException(nameof(propInfo));

            var value = propInfo.GetValue(instance);

            EnsureValidContext(value, instance, propInfo.EntityType);

            if (null == value && NullIsAlwaysValid) return null;

            if (IsDisabled(instance)) return null;

            var otherProperties = new Dictionary<string, (object Value, string DisplayName)>();
            if (null != instance && null != DependsOnOtherProperties && DependsOnOtherProperties.Count > 0)
            {
                foreach (var propName in DependsOnOtherProperties)
                {
                    var extraPropInfo = relatedProps.FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
                    if (null == extraPropInfo) throw new InvalidOperationException(
                        $"No property {propName} on type {propInfo.EntityType.FullName}.");
                    otherProperties.Add(propName, (extraPropInfo.GetValue(instance), extraPropInfo.GetDisplayName()));
                }
            }

            var result = GetValidationResult(value, instance, propInfo.EntityType,
                propInfo.GetDisplayName(), otherProperties);

            if (null == result) return null;

            return new BrokenRule(this.GetType().FullName, propInfo.Name, result.Description, result.Severity);
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (null == validationContext) throw new ArgumentNullException(nameof(validationContext));
            EnsureValidContext(value, validationContext.ObjectInstance, validationContext.ObjectType);

            if (null == value && NullIsAlwaysValid) return ValidationResult.Success;

            if (IsDisabled(validationContext.ObjectInstance)) return ValidationResult.Success;

            var propDisplayName = validationContext.DisplayName.IsNullOrWhiteSpace()
                ? validationContext.GetPropertyDisplayName()
                : validationContext.DisplayName;

            var otherProperties = new Dictionary<string, (object Value, string DisplayName)>();
            if (null != validationContext.ObjectInstance && null != DependsOnOtherProperties && DependsOnOtherProperties.Count > 0)
            {
                foreach (var propName in DependsOnOtherProperties)
                {
                    var extraPropInfo = validationContext.ObjectType.GetProperties()
                        .FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));

                    if (null == extraPropInfo) throw new InvalidOperationException(
                        $"No property {propName} on type {validationContext.ObjectType.FullName}.");

                    var displayAttr = extraPropInfo.GetCustomAttributeWithInheritance<DisplayAttribute>();
                    var displayName = displayAttr?.GetName() ?? propName;

                    otherProperties.Add(propName, (extraPropInfo.GetValue(validationContext.ObjectInstance), displayName));
                }
            }

            var result = GetValidationResult(value, validationContext.ObjectInstance,
                validationContext.ObjectType, propDisplayName, otherProperties);

            if (null == result || result.Severity != RuleSeverity.Error) return ValidationResult.Success;

            return new ValidationResult(result.Description);
        }

        /// <summary>
        /// Override the method to perform actual validation if the rule can produce
        /// different severity broken rules.
        /// </summary>
        /// <param name="value">a value of the property to validate</param>
        /// <param name="entityInstance">an entity instance to validate</param>
        /// <param name="entityType">a type of the entity instance to validate
        /// (because if entityInstance is null, no way to get its type)</param>
        /// <param name="propertyDisplayName">a display name of the property to use in an error description</param>
        /// <param name="otherProperties">other properties that the validation rule depends on
        /// (if any), see <see cref="DependsOnOtherProperties"/></param>
        /// <returns>a <see cref="BrokenRuleResult"/> - if the property value is invalid;
        /// null - if the property value is valid.</returns>
        /// <remarks>Either <see cref="GetValidationResult"/> or <see cref="GetErrorDescripton"/>
        /// should be overriden by the rule implementation.</remarks>
        protected virtual BrokenRuleResult GetValidationResult(object value, object entityInstance,
            Type entityType, string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var description = GetErrorDescripton(value, entityInstance, entityType, propertyDisplayName, otherProperties);

            if (description.IsNullOrWhiteSpace()) return null;

            return new BrokenRuleResult(description, Severity);
        }

        /// <summary>
        /// Override the method to perform actual validation if the rule only produces
        /// broken rules of the severity specified by <see cref="Severity"/> property.
        /// </summary>
        /// <param name="value">a value of the property to validate</param>
        /// <param name="entityInstance">an entity instance to validate</param>
        /// <param name="entityType">a type of the entity instance to validate
        /// (because if entityInstance is null, no way to get its type)</param>
        /// <param name="propertyDisplayName">a display name of the property to use in an error description</param>
        /// <param name="otherProperties">other properties that the validation rule depends on
        /// (if any), see <see cref="DependsOnOtherProperties"/></param>
        /// <returns>a broken rule description - if the property value is invalid;
        /// null or empty string - if the property value is valid.</returns>
        /// <remarks>Either <see cref="GetValidationResult"/> or <see cref="GetErrorDescripton"/>
        /// should be overriden by the rule implementation.</remarks>
        protected virtual string GetErrorDescripton(object value, object entityInstance,
            Type entityType, string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties) => null;

        #endregion

        #region Helpers

        private void EnsureValidContext(object value, object entityInstance, Type entityType)
        {
            if (null != value && null != SupportedValueTypes && SupportedValueTypes.Length > 0 &&
                !SupportedValueTypes.Any(t => t.IsAssignableFrom(value.GetType())))
                throw new InvalidOperationException($"Property value type {value.GetType().FullName} " +
                    $"is not allowed for rule {this.GetType().FullName} (affected entity type " +
                    $"- {entityType?.FullName ?? "not specified"}).");

            if (null == entityInstance && EntityInstanceRequired)
                throw new InvalidOperationException($"Validated entity instance is not specified for rule {this.GetType().FullName}.");

            if (null != SupportedEntityType && !SupportedEntityType.IsAssignableFrom(entityType))
                throw new InvalidOperationException($"Rule {this.GetType().FullName} does not support validated entity type {entityType.FullName}.");
        }

        private bool IsDisabled(object instance)
        {
            if (EnabledPropertyName.IsNullOrWhiteSpace() || null == instance) return false;

            var enabledPropInfo = instance.GetType().GetProperty(EnabledPropertyName);

            if (null == enabledPropInfo) throw new InvalidOperationException(
                $"Property {EnabledPropertyName} does not exist on type {instance.GetType().FullName}.");
            if (enabledPropInfo.PropertyType != typeof(bool)) throw new InvalidOperationException(
                $"Rule switch property {EnabledPropertyName} for entity type {instance.GetType().FullName} is not bool.");

            return !(bool)enabledPropInfo.GetValue(instance);
        }

        #endregion

        /// <summary>
        /// A minimal broken rule result to support both <see cref="ValidationAttribute.IsValid(object, ValidationContext)"/>
        /// and <see cref="IPropertyValidationAttribute.GetValidationResult"/> methods.
        /// </summary>
        protected class BrokenRuleResult
        {
            /// <summary>
            /// A minimal broken rule result to support both <see cref="ValidationAttribute.IsValid(object, ValidationContext)"/>
            /// and <see cref="IPropertyValidationAttribute.GetValidationResult"/> methods.
            /// </summary>
            /// <param name="description">description for <see cref="BrokenRule.Description"/> and
            /// <see cref="ValidationResult.ErrorMessage"/> properties</param>
            /// <param name="severity">value for <see cref="BrokenRule.Severity"/> property</param>
            public BrokenRuleResult(string description, RuleSeverity severity)
            {
                if (description.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(description));

                Description = description;
                Severity = severity;
            }

            /// <summary>
            /// A description for <see cref="BrokenRule.Description"/> and <see cref="ValidationResult.ErrorMessage"/>
            /// properties.
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// A value for <see cref="BrokenRule.Severity"/> property.
            /// </summary>
            public RuleSeverity Severity { get; }
        }
    }
}

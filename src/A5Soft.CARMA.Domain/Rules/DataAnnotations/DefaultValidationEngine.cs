using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Domain.Reflection;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// A default implementation of validation engine using validation attributes.
    /// </summary>
    internal class DefaultValidationEngine : IValidationEngine
    {
        #region Fields

        private readonly ReadOnlyCollection<EntityValidationRule> _entityValidationRules;
        private readonly ReadOnlyDictionary<string, List<PropertyValidationRule>> _propertyValidationRules;
        private readonly Dictionary<string, List<string>> _validationDependencies = new Dictionary<string, List<string>>();

        #endregion

        #region Constructor
             
        public DefaultValidationEngine(IEntityMetadata metadata)
        {
            EntityMetadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            if (typeof(IDomainEntityReference).IsAssignableFrom(metadata.EntityType)) throw new ArgumentException(
                $"Cannot create validation engine for reference to an entity {EntityMetadata.EntityType.FullName}.");

            _entityValidationRules = new ReadOnlyCollection<EntityValidationRule>(EntityMetadata.EntityType
                .GetCustomAttributesWithInheritance<ValidationAttribute>()
                .Select(a => new EntityValidationRule(a)).ToList());

            var relevantProperties = EntityMetadata.EntityType.GetProperties()
                .Where(p => EntityMetadata.Properties.ContainsKey(p.Name));

            var validationRules = new Dictionary<string, List<PropertyValidationRule>>();
            foreach (var propInfo in relevantProperties)
            {
                validationRules.Add(propInfo.Name, GetPropertyValidationRules(propInfo));
            }
            _propertyValidationRules = new ReadOnlyDictionary<string, List<PropertyValidationRule>>(validationRules);

            InitValidationDependencies();
        }

        private static List<PropertyValidationRule> GetPropertyValidationRules(PropertyInfo propInfo)
        {
            return propInfo.GetCustomAttributesWithInheritance<ValidationAttribute>()
                .Select(a => new PropertyValidationRule(a)).ToList();
        }

        private void InitValidationDependencies()
        {
            foreach (var propertyRuleList in _propertyValidationRules)
            {
                var dependentProps = new List<string>();
                foreach (var validationRule in propertyRuleList.Value)
                {
                    dependentProps.AddRange(validationRule.GetRelatedProperties());
                }

                dependentProps = dependentProps.Distinct().ToList();

                foreach (var dependentProp in dependentProps)
                {
                    if (_validationDependencies.ContainsKey(dependentProp))
                    {
                        _validationDependencies[dependentProp].Add(propertyRuleList.Key);
                    }
                    else
                    {
                        _validationDependencies.Add(dependentProp, new List<string>() { propertyRuleList.Key });
                    }
                }
            }
        }

        #endregion


        /// <inheritdoc cref="IValidationEngine.EntityMetadata" />
        public IEntityMetadata EntityMetadata { get; }


        /// <inheritdoc cref="IValidationEngine.GetBrokenRules" />
        public List<BrokenRule> GetBrokenRules(object instance, string propName)
        {
            if (propName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propName));

            EnsureInstanceIsValid(instance);

            if (!EntityMetadata.Properties.ContainsKey(propName)) throw new ArgumentException(
                $"Property {propName} does not exist for type {EntityMetadata.EntityType.FullName}.", 
                nameof(propName));

            if (!_propertyValidationRules.ContainsKey(propName))
                return new List<BrokenRule>();

            var propInfo = EntityMetadata.Properties[propName];

            IEnumerable<IPropertyMetadata> dependentProps;
            if (!_validationDependencies.ContainsKey(propName))
                dependentProps = new List<IPropertyMetadata>();
            else
                dependentProps = _validationDependencies[propName]
                .Select(n => (IPropertyMetadata)EntityMetadata.Properties.ValueOrDefault(n))
                .Where(p => !p.IsNull());

            var result = new List<BrokenRule>();
            foreach (var propertyValidationRule in _propertyValidationRules[propName])
            {
                var brokenRule = propertyValidationRule.GetValidationResult(
                    instance, propInfo, EntityMetadata.Properties.Select(kv => kv.Value));
                if (!brokenRule.IsNull()) result.Add(brokenRule);
            }

            return result;
        }

        /// <inheritdoc cref="IValidationEngine.GetBrokenRules" />
        public List<BrokenRule> GetBrokenRules(object instance, string propName, bool includeDependentProps)
        {
            var result = GetBrokenRules(instance, propName);

            if (includeDependentProps && _validationDependencies.ContainsKey(propName))
            {
                var dependantProps = _validationDependencies[propName]
                    .Where(n => _propertyValidationRules.ContainsKey(n));
                foreach (var dependentPropName in dependantProps)
                {
                    result.AddRange(GetBrokenRules(instance, dependentPropName));
                }
            }

            return result;
        }

        /// <inheritdoc cref="IValidationEngine.GetBrokenRules" />
        public List<BrokenRule> GetBrokenRules(object instance)
        {
            EnsureInstanceIsValid(instance);

            var result = new List<BrokenRule>();
            foreach (var validator in _entityValidationRules)
            {
                var brokenRule = validator.GetValidationResult(instance, EntityMetadata);
                if (!brokenRule.IsNull()) result.Add(brokenRule);
            }

            return result;
        }

        /// <inheritdoc cref="IValidationEngine.GetAllBrokenRules" />
        public List<BrokenRule> GetAllBrokenRules(object instance)
        {
            EnsureInstanceIsValid(instance);

            var result = new List<BrokenRule>();

            result.AddRange(GetBrokenRules(instance));

            foreach (var propertyMetadata in EntityMetadata.Properties)
            {
                result.AddRange(GetBrokenRules(instance, propertyMetadata.Key));
            }

            return result;
        }

        /// <inheritdoc cref="IValidationEngine.GetDependentPropertyNames" />
        public string[] GetDependentPropertyNames(string propName)
        {
            if (propName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propName));

            if (!_validationDependencies.ContainsKey(propName)) return new string[] { };

            return _validationDependencies[propName]
                .Where(p => _propertyValidationRules.ContainsKey(p))
                .ToArray();
        }


        private void EnsureInstanceIsValid(object instance)
        {
            if (instance.IsNull()) throw new ArgumentNullException(nameof(instance));
            if (!EntityMetadata.EntityType.IsAssignableFrom(instance.GetType())) throw new ArgumentException(
                $"Instance of type {instance.GetType().FullName} is not assignable to entity type {EntityMetadata.EntityType.FullName}.",
                    nameof(instance));
        }

    }
}

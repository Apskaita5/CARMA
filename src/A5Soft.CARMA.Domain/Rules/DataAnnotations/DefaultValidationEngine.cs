using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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
        private readonly Dictionary<string, HashSet<string>> _reverseDependencyMap =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

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
                var rulesForProperty = GetPropertyValidationRules(propInfo);

                validationRules.Add(propInfo.Name, rulesForProperty);

                foreach (var rule in rulesForProperty)
                {
                    foreach (var dependencySource in rule.GetRelatedProperties())
                    {
                        if (!_reverseDependencyMap.TryGetValue(dependencySource, out var dependants))
                        {
                            dependants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            _reverseDependencyMap[dependencySource] = dependants;
                        }

                        if (!dependants.Contains(propInfo.Name)) dependants.Add(propInfo.Name);
                    }
                }
            }
            _propertyValidationRules = new ReadOnlyDictionary<string, List<PropertyValidationRule>>(validationRules);
        }

        private static List<PropertyValidationRule> GetPropertyValidationRules(PropertyInfo propInfo)
        {
            return propInfo.GetCustomAttributesWithInheritance<ValidationAttribute>()
                .Select(a => new PropertyValidationRule(a)).ToList();
        }

        #endregion


        /// <inheritdoc cref="IValidationEngine.EntityMetadata" />
        public IEntityMetadata EntityMetadata { get; }


        /// <inheritdoc cref="IValidationEngine.GetBrokenRules(object, string)" />
        public List<BrokenRule> GetBrokenRules(object instance, string propName)
        {
            EnsureInstanceIsValid(instance);
            EnsurePropertyIsValid(propName);

            return GetBrokenRulesForProperty(instance, propName);
        }

        /// <inheritdoc cref="IValidationEngine.GetBrokenRules(object, string, bool)" />
        public List<BrokenRule> GetBrokenRules(object instance, string propName, bool includeDependentProps)
        {
            EnsureInstanceIsValid(instance);
            EnsurePropertyIsValid(propName);

            if (!includeDependentProps) return GetBrokenRulesForProperty(instance, propName);

            var result = new List<BrokenRule>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ValidateRecursive(instance, propName, result, visited);

            return result;
        }

        /// <inheritdoc cref="IValidationEngine.GetBrokenRules(object)" />
        public List<BrokenRule> GetBrokenRules(object instance)
        {
            EnsureInstanceIsValid(instance);

            var result = new List<BrokenRule>();
            foreach (var validator in _entityValidationRules)
            {
                var brokenRule = validator.GetValidationResult(instance, EntityMetadata);
                if (null != brokenRule) result.Add(brokenRule);
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
                result.AddRange(GetBrokenRulesForProperty(instance, propertyMetadata.Key));
            }

            return result;
        }

        /// <inheritdoc cref="IValidationEngine.GetDependentPropertyNames" />
        public string[] GetDependentPropertyNames(string propName)
        {
            if (propName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propName));

            if (_reverseDependencyMap.TryGetValue(propName, out var result))
                return result.ToArray();

            return new string[] { };
        }


        private List<BrokenRule> GetBrokenRulesForProperty(object instance, string propName)
        {
            if (!_propertyValidationRules.TryGetValue(propName, out var rules)
                || rules.Count < 1) return new List<BrokenRule>();

            var propInfo = EntityMetadata.Properties[propName];

            var result = new List<BrokenRule>();
            foreach (var rule in rules)
            {
                var brokenRule = rule.GetValidationResult(
                    instance, propInfo, EntityMetadata.Properties.Select(kv => kv.Value));
                if (null != brokenRule) result.Add(brokenRule);
            }

            return result;
        }

        private void ValidateRecursive(object instance, string propertyName,
            List<BrokenRule> brokenRules, HashSet<string> visited)
        {
            if (!visited.Add(propertyName))
                return; // prevent cycles

            // Validate the requested property
            brokenRules.AddRange(GetBrokenRulesForProperty(instance, propertyName));

            // Find dependants
            if (_reverseDependencyMap.TryGetValue(propertyName, out var dependants)
                && dependants.Count > 0)
            {
                foreach (var dependant in dependants)
                {
                    ValidateRecursive(instance, dependant, brokenRules, visited);
                }
            }
        }

        private void EnsureInstanceIsValid(object instance)
        {
            if (instance.IsNull()) throw new ArgumentNullException(nameof(instance));
            if (!EntityMetadata.EntityType.IsAssignableFrom(instance.GetType())) throw new ArgumentException(
                $"Instance of type {instance.GetType().FullName} is not assignable to entity type {EntityMetadata.EntityType.FullName}.",
                    nameof(instance));
        }

        private void EnsurePropertyIsValid(string propName)
        {
            if (propName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propName));
            if (!EntityMetadata.Properties.ContainsKey(propName)) throw new ArgumentException(
                $"Property {propName} does not exist for type {EntityMetadata.EntityType.FullName}.",
                nameof(propName));
        }
    }
}

using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// A default implementation of validation engine using validation attributes.
    /// </summary>
    internal class DefaultValidationEngine : IValidationEngine
    {
        #region Fields

        private readonly EntityRulesMetadata _rulesMetadata;
        private readonly Dictionary<Type, IValidationRule> _rules;

        #endregion

        #region Constructor

        internal DefaultValidationEngine(EntityRulesMetadata rulesMetadata,
            Func<Type, IValidationRule> resolveValidationRule)
        {
            if (null == resolveValidationRule) throw new ArgumentNullException(nameof(resolveValidationRule));

            _rulesMetadata = rulesMetadata ?? throw new ArgumentNullException(nameof(rulesMetadata));
            _rules = rulesMetadata.RequiredRules.ToDictionary(t => t, t => resolveValidationRule(t));
        }

        #endregion


        /// <inheritdoc cref="IValidationEngine.EntityMetadata" />
        public IEntityMetadata EntityMetadata => _rulesMetadata.EntityMetadata;


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

            var result = new List<BrokenRule>();

            // Validate the requested property
            result.AddRange(GetBrokenRulesForProperty(instance, propName));

            if (!includeDependentProps) return result;

            foreach (var dependant in _rulesMetadata.GetDependantPropertiesFor(propName))
            {
                result.AddRange(GetBrokenRulesForProperty(instance, dependant));
            }

            return result;
        }

        /// <inheritdoc cref="IValidationEngine.GetBrokenRules(object)" />
        public List<BrokenRule> GetBrokenRules(object instance)
        {
            EnsureInstanceIsValid(instance);

            var result = new List<BrokenRule>();
            foreach (var ruleMetadata in _rulesMetadata.EntityValidationRules)
            {
                if (_rules.TryGetValue(ruleMetadata.RuleType, out var validator))
                {
                    var brokenRule = validator.GetValidationResult(
                        new ValidationContext(instance, EntityMetadata, ruleMetadata));
                    if (null != brokenRule) result.Add(brokenRule);
                }
                else throw new InvalidOperationException(
                    $"Validation rule of type {ruleMetadata.RuleType.FullName} is not configured.");
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

            return _rulesMetadata.GetDependantPropertiesFor(propName);
        }


        private List<BrokenRule> GetBrokenRulesForProperty(object instance, string propName)
        {
            var result = new List<BrokenRule>();
            var rules = _rulesMetadata.GetRulesFor(propName);

            if (rules.Count < 1) return result;

            if (!EntityMetadata.Properties.TryGetValue(propName, out var propInfo))
                throw new InvalidOperationException($"No metadata defined for property {propName} of {EntityMetadata.EntityType.FullName}.");

            foreach (var rule in rules)
            {
                if (_rules.TryGetValue(rule.RuleType, out var validator))
                {
                    var brokenRule = validator.GetValidationResult(
                        new ValidationContext(instance, _rulesMetadata.EntityMetadata, rule, propInfo));
                    if (null != brokenRule) result.Add(brokenRule);
                }
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
            foreach (var dependant in _rulesMetadata.GetDependantPropertiesFor(propertyName))
            {
                ValidateRecursive(instance, dependant, brokenRules, visited);
            }
        }

        private void EnsureInstanceIsValid(object instance)
        {
            if (null == instance) throw new ArgumentNullException(nameof(instance));
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

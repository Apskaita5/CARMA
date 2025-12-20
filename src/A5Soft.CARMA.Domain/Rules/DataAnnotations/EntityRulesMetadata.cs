using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Validation rules metadata container using validation attributes.
    /// </summary>
    internal class EntityRulesMetadata
    {
        #region Fields

        private static readonly string[] _noDependants = new string[] { };
        private static readonly List<IRuleMetadata> _noRules = new List<IRuleMetadata>();
        private readonly ReadOnlyDictionary<string, List<IRuleMetadata>> _propertyValidationRules;
        private readonly Dictionary<string, HashSet<string>> _reverseDependencyMap =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructor

        public EntityRulesMetadata(IEntityMetadata metadata)
        {
            EntityMetadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            if (typeof(IDomainEntityReference).IsAssignableFrom(metadata.EntityType)) throw new ArgumentException(
                $"Cannot create validation engine for reference to an entity {EntityMetadata.EntityType.FullName}.");

            var requiredRules = new HashSet<Type>();

            EntityValidationRules = new ReadOnlyCollection<IRuleMetadata>(EntityMetadata.EntityType
                .GetCustomAttributesWithInheritance<EntityValidationAttribute>()
                .Select(a => (IRuleMetadata)a).ToList());

            foreach (var rule in EntityValidationRules)
            {
                if (!requiredRules.Contains(rule.RuleType)) requiredRules.Add(rule.RuleType);
            }

            var relevantProperties = EntityMetadata.EntityType.GetProperties()
                .Where(p => EntityMetadata.Properties.ContainsKey(p.Name));

            var validationRules = new Dictionary<string, List<IRuleMetadata>>();
            foreach (var propInfo in relevantProperties)
            {
                var rulesForProperty = propInfo.GetCustomAttributesWithInheritance<PropertyValidationAttribute>()
                    .Select(a => (IRuleMetadata)a).ToList();

                validationRules.Add(propInfo.Name, rulesForProperty);

                foreach (var rule in rulesForProperty)
                {
                    if (!requiredRules.Contains(rule.RuleType)) requiredRules.Add(rule.RuleType);

                    foreach (var dependencySource in rule.RelatedProperties)
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

            _propertyValidationRules = new ReadOnlyDictionary<string, List<IRuleMetadata>>(validationRules);
            RequiredRules = requiredRules.ToArray();
        }

        #endregion

        #region Properties

        public IEntityMetadata EntityMetadata { get; }

        public Type[] RequiredRules { get; }

        public ReadOnlyCollection<IRuleMetadata> EntityValidationRules { get; }

        #endregion

        #region Methods

        public string[] GetDependantPropertiesFor(string propName)
        {
            if (_reverseDependencyMap.TryGetValue(propName, out var dependants)
                && dependants.Count > 0) return dependants.ToArray();
            return _noDependants;
        }

        public IReadOnlyList<IRuleMetadata> GetRulesFor(string propName)
        {
            if (_propertyValidationRules.TryGetValue(propName, out var result))
                return result;
            return _noRules;
        }

        #endregion
    }
}

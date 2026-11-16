using System;
using System.Collections.Generic;
using System.Linq;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules.DataAnnotations;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// A broken rules manager for a business entity.
    /// </summary>
    [Serializable]
    internal sealed class BrokenRulesManager<T> : BrokenRules
        where T: DomainObject<T>
    {
        private readonly T _parent;
        private readonly IValidationEngine _engine;


        public BrokenRulesManager(T parent, IValidationEngineProvider validationProvider)
        {
            if (null == validationProvider) throw new ArgumentNullException(nameof(validationProvider));

            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _engine = validationProvider.GetValidationEngine(parent.GetType())
                ?? throw new InvalidOperationException($"Validation provider returned null engine for domain object type {typeof(T).FullName}.");
            if (null == _engine.EntityMetadata) throw new InvalidOperationException(
                $"Engine for domain object type {typeof(T).FullName} does not include entity metadata.");
        }

        private BrokenRulesManager(T forNewParent, IValidationEngine validationEngine)
        {
            _parent = forNewParent ?? throw new ArgumentNullException(nameof(forNewParent));
            _engine = validationEngine;
        }


        public IEntityMetadata Metadata => _engine.EntityMetadata;


        public HashSet<string> CheckPropertyRules(string propertyName)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            var result = new HashSet<string>(StringComparer.Ordinal) { propertyName };
            foreach (var dependantProp in _engine.GetDependentPropertyNames(propertyName))
            {
                _ = result.Add(dependantProp);
            }

            Replace(r => result.Contains(r.Property),
                _engine.GetBrokenRules(_parent, propertyName, true));

            return result;
        }

        public void CheckObjectRules()
        {
            ReplaceAll(_engine.GetAllBrokenRules(_parent));
        }

        public BrokenRulesManager<T> CreateForParent(T forNewParent)
        {
             return new BrokenRulesManager<T>(forNewParent, _engine);
        }
    }
}

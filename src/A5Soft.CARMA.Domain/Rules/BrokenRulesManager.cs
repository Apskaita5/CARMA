using System;
using System.Collections.Generic;
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
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _engine = validationProvider?.GetValidationEngine(parent.GetType())
                ?? DefaultValidationEngineProvider.GetDefaultValidationEngine<T>();
            if (null == _engine.EntityMetadata) throw new InvalidOperationException("Engine must include entity metadata.");
        }

        private BrokenRulesManager(T forNewParent, IValidationEngine validationEngine)
        {
            _parent = forNewParent ?? throw new ArgumentNullException(nameof(forNewParent));
            _engine = validationEngine;
        }


        public IEntityMetadata Metadata => _engine.EntityMetadata;


        public string[] CheckPropertyRules(string propertyName)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            var result = new List<string>() { propertyName };
            result.AddRange(_engine.GetDependentPropertyNames(propertyName));

            ClearBrokenRulesForProperties(result);

            _brokenRules.AddRange(_engine.GetBrokenRules(_parent, propertyName, true));

            ResetCount();

            return result.ToArray();
        }

        public void CheckObjectRules()
        {
            _brokenRules.Clear();
            _brokenRules.AddRange(_engine.GetAllBrokenRules(_parent));
            ResetCount();
        }

        public BrokenRulesManager<T> GetCopy(T forNewParent)
        {
             return new BrokenRulesManager<T>(forNewParent, _engine);
        }


        private void ClearBrokenRulesForProperties(List<string> propertyNames)
        {
            for (int i = _brokenRules.Count - 1; i > - 1; i--)
            {
                 if (propertyNames.Contains(_brokenRules[i].Property)) _brokenRules.RemoveAt(i);
            }
        }
    }
}

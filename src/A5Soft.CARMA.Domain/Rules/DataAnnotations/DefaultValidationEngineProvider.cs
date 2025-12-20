using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Concurrent;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Default implementation of validation engine provider using validation attributes.
    /// </summary>
    [DefaultServiceImplementation(typeof(IValidationEngineProvider))]
    public class DefaultValidationEngineProvider : IValidationEngineProvider
    {
        private static readonly ConcurrentDictionary<Type, EntityRulesMetadata> _cache
            = new ConcurrentDictionary<Type, EntityRulesMetadata>();
        private readonly IMetadataProvider _metadataProvider;
        private readonly IRuleProvider _ruleProvider;


        public DefaultValidationEngineProvider(IMetadataProvider metadataProvider, IRuleProvider ruleProvider)
        {
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
            _ruleProvider = ruleProvider ?? throw new ArgumentNullException(nameof(ruleProvider));
        }


        /// <inheritdoc cref="IValidationEngineProvider.GetValidationEngine" />
        public IValidationEngine GetValidationEngine(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));

            var metadata = _cache.GetOrAdd(entityType, t => new EntityRulesMetadata(
                _metadataProvider.GetEntityMetadata(t)));
            return new DefaultValidationEngine(metadata, t => _ruleProvider.ResolveRule(t));
        }

        /// <inheritdoc cref="IValidationEngineProvider.GetValidationEngine{T}" />
        public IValidationEngine GetValidationEngine<T>() where T : class
        {
            return GetValidationEngine(typeof(T));
        }
    }
}

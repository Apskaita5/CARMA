using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Default implementation of validation engine provider using validation attributes.
    /// </summary>
    [DefaultServiceImplementation(typeof(IValidationEngineProvider))]
    public class DefaultValidationEngineProvider : IValidationEngineProvider
    {
        // shared across scopes, one cache per metadata provider instance;
        // entries go away when the metadata provider is garbage-collected
        private static readonly ConditionalWeakTable<IMetadataProvider,
            ConcurrentDictionary<Type, EntityRulesMetadata>> _metadataCaches
            = new ConditionalWeakTable<IMetadataProvider, ConcurrentDictionary<Type, EntityRulesMetadata>>();

        // per scope (provider instance)
        private readonly ConcurrentDictionary<Type, IValidationEngine> _engines
            = new ConcurrentDictionary<Type, IValidationEngine>();
        private readonly ConcurrentDictionary<Type, EntityRulesMetadata> _metadataCache
            = new ConcurrentDictionary<Type, EntityRulesMetadata>();
        private readonly IMetadataProvider _metadataProvider;
        private readonly IRuleProvider _ruleProvider;


        public DefaultValidationEngineProvider(IMetadataProvider metadataProvider, IRuleProvider ruleProvider)
        {
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
            _ruleProvider = ruleProvider ?? throw new ArgumentNullException(nameof(ruleProvider));
            _metadataCache = _metadataCaches.GetValue(_metadataProvider,
                _ => new ConcurrentDictionary<Type, EntityRulesMetadata>());
        }


        /// <inheritdoc cref="IValidationEngineProvider.GetValidationEngine" />
        public IValidationEngine GetValidationEngine(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));

            return _engines.GetOrAdd(entityType, CreateEngine);
        }

        /// <inheritdoc cref="IValidationEngineProvider.GetValidationEngine{T}" />
        public IValidationEngine GetValidationEngine<T>() where T : class
        {
            return GetValidationEngine(typeof(T));
        }


        private IValidationEngine CreateEngine(Type entityType)
        {
            var metadata = _metadataCache.GetOrAdd(entityType,
                t => new EntityRulesMetadata(_metadataProvider.GetEntityMetadata(t)));
            return new DefaultValidationEngine(metadata, _ruleProvider.ResolveRule);
        }
    }
}

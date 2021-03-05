using System;
using System.Collections.Concurrent;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Default implementation of validation engine provider using validation attributes.
    /// </summary>
    public class DefaultValidationEngineProvider : IValidationEngineProvider
    {
        private static readonly ConcurrentDictionary<Type, DefaultValidationEngine> _Cache
            = new ConcurrentDictionary<Type, DefaultValidationEngine>();
        private static readonly ConcurrentDictionary<Type, DefaultValidationEngine> _CacheForDefaultValidationEngines
            = new ConcurrentDictionary<Type, DefaultValidationEngine>();

        private readonly IMetadataProvider _metadataProvider;


        public DefaultValidationEngineProvider(IMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
        }


        /// <inheritdoc cref="IValidationEngineProvider.GetValidationEngine" />
        public IValidationEngine GetValidationEngine(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));
            return _Cache.GetOrAdd(entityType, t => new DefaultValidationEngine(
                _metadataProvider.GetEntityMetadata(t)));
        }

        /// <inheritdoc cref="IValidationEngineProvider.GetValidationEngine{T}" />
        public IValidationEngine GetValidationEngine<T>() where T : class
        {
            return GetValidationEngine(typeof(T));
        }


        internal static IValidationEngine GetDefaultValidationEngine(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));
            return _CacheForDefaultValidationEngines.GetOrAdd(entityType, t => new DefaultValidationEngine(
                DefaultMetadataProvider.GetDefaultEntityMetadata(t)));
        }

        internal static IValidationEngine GetDefaultValidationEngine<T>()
        {
            return GetDefaultValidationEngine(typeof(T));
        }

    }
}

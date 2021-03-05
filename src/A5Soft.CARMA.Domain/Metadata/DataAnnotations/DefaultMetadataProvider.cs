using System;
using System.Collections.Concurrent;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Default implementation of IMetadataProvider using validation attributes.
    /// </summary>
    public class DefaultMetadataProvider : IMetadataProvider
    {
        private static readonly ConcurrentDictionary<Type, EntityMetadata> _cache
            = new ConcurrentDictionary<Type, EntityMetadata>();


        /// <inheritdoc cref="IMetadataProvider.GetEntityMetadata" />
        public IEntityMetadata GetEntityMetadata(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));
            return GetDefaultEntityMetadata(entityType);
        }

        /// <inheritdoc cref="IMetadataProvider.GetEntityMetadata" />
        public IEntityMetadata GetEntityMetadata<T>()
        {
            return GetDefaultEntityMetadata<T>();
        }


        internal static IEntityMetadata GetDefaultEntityMetadata(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));
            return _cache.GetOrAdd(entityType, t => new EntityMetadata(t));
        }

        internal static IEntityMetadata GetDefaultEntityMetadata<T>()
        {
            return GetDefaultEntityMetadata(typeof(T));
        }

    }
}

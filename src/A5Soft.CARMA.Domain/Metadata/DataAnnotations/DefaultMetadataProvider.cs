using System;
using System.Collections.Concurrent;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Default implementation of IMetadataProvider using validation attributes.
    /// </summary>
    [DefaultServiceImplementation(typeof(IMetadataProvider))]
    public class DefaultMetadataProvider : IMetadataProvider
    {
        private static readonly ConcurrentDictionary<Type, EntityMetadata> _entityCache
            = new ConcurrentDictionary<Type, EntityMetadata>();
        private static readonly ConcurrentDictionary<Type, UseCaseMetadata> _useCaseCache
            = new ConcurrentDictionary<Type, UseCaseMetadata>();


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

        /// <inheritdoc cref="IMetadataProvider.GetUseCaseMetadata" />
        public IUseCaseMetadata GetUseCaseMetadata(Type useCaseType)
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));
            return GetDefaultUseCaseMetadata(useCaseType);
        }

        /// <inheritdoc cref="IMetadataProvider.GetUseCaseMetadata" />
        public IUseCaseMetadata GetUseCaseMetadata<T>()
        {
            return GetDefaultUseCaseMetadata<T>();
        }


        internal static IEntityMetadata GetDefaultEntityMetadata(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));
            return _entityCache.GetOrAdd(entityType, t => new EntityMetadata(t));
        }

        internal static IEntityMetadata GetDefaultEntityMetadata<T>()
        {
            return GetDefaultEntityMetadata(typeof(T));
        }

        private static IUseCaseMetadata GetDefaultUseCaseMetadata(Type useCaseType)
        {
            return _useCaseCache.GetOrAdd(useCaseType, t => new UseCaseMetadata(t));
        }

        private static IUseCaseMetadata GetDefaultUseCaseMetadata<T>()
        {
            return GetDefaultUseCaseMetadata(typeof(T));
        }
    }
}

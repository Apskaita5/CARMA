using System;

namespace A5Soft.CARMA.Domain.Metadata
{
    /// <summary>
    /// Common interface for metadata providers (services) for use in dependency injection.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets metadata for the entity type specified.
        /// </summary>
        /// <param name="entityType">a type of the entity to get metadata for</param>
        /// <returns>metadata for the entity type specified</returns>
        IEntityMetadata GetEntityMetadata(Type entityType);

        /// <summary>
        /// Gets metadata for the entity type specified.
        /// </summary>
        /// <typeparam name="T">a type of the entity to get metadata for</typeparam>
        /// <returns>metadata for the entity type specified</returns>
        IEntityMetadata GetEntityMetadata<T>();

        /// <summary>
        /// Gets metadata for the use case type specified.
        /// </summary>
        /// <param name="useCaseType">a type of the use case to get metadata for</param>
        /// <returns>metadata for the use case type specified</returns>
        IUseCaseMetadata GetUseCaseMetadata(Type useCaseType);

        /// <summary>
        /// Gets metadata for the use case type specified.
        /// </summary>
        /// <typeparam name="T">a type of the use case to get metadata for</typeparam>
        /// <returns>metadata for the use case type specified</returns>
        IUseCaseMetadata GetUseCaseMetadata<T>();
    }
}

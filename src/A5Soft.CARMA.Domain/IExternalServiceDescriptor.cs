using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Create classes with the interface to provide external service desciptors for IoC.
    /// </summary>
    public interface IExternalServiceDescriptor
    {
        /// <summary>
        /// a type of the service implementation (class)
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// a type of the service interface (could be null if registered only by implementation type)
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// a configuration of the app build (client/server/any) that the implementation is meant for
        /// </summary>
        public BuildConfiguration ForBuildConfiguration { get; }

        /// <summary>
        /// a lifetime of the service within a IoC container
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// whether multiple implementations of the service are allowed (not only a single)
        /// </summary>
        public bool AllowMultipleImplementations { get; }
    }
}

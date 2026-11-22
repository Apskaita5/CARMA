using System;

namespace A5Soft.CARMA.Domain.IoC
{
    /// <summary>
    /// Information about a required service that is defined in an external managed assembly for IoC.
    /// </summary>
    public sealed class ExternalServiceDescriptor
    {
        /// <summary>
        /// Information about a required service that is defined in an external managed assembly for IoC.
        /// </summary>
        /// <param name="implementationType">a type of the service implementation (class)</param>
        /// <param name="interfaceType">a type of the service interface (could be null if registered only by implementation type)</param>
        /// <param name="forBuildConfiguration">a configuration of the app build (client/server/any) that the implementation is meant for</param>
        /// <param name="lifetime">a lifetime of the service within a IoC container</param>
        /// <param name="allowMultipleImplementations">whether multiple implementations of the service are allowed (not only a single)</param>
        public ExternalServiceDescriptor(Type implementationType, Type interfaceType, ServiceLifetime lifetime,
            BuildConfiguration forBuildConfiguration, bool allowMultipleImplementations = false)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            if (!ImplementationType.IsClass || ImplementationType.IsAbstract) throw new ArgumentException(
                $"Implementation type must be non abstract class, while {ImplementationType.FullName} is not.",
                nameof(ImplementationType));

            InterfaceType = interfaceType;
            if (null != InterfaceType && !InterfaceType.IsAssignableFrom(ImplementationType))
                throw new ArgumentException($"Implementation {ImplementationType.FullName} is not assignable to {InterfaceType.FullName}.",
                    nameof(InterfaceType));

            ForBuildConfiguration = forBuildConfiguration;
            Lifetime = lifetime;
            AllowMultipleImplementations = allowMultipleImplementations && null != InterfaceType;
        }

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

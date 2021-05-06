using System;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Basic info about an application service for DI.
    /// </summary>
    public class ApplicationServiceInfo
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="interfaceType">a type of the service (interface)</param>
        /// <param name="implementationType">a service (interface) implementation</param>
        /// <param name="lifetime">a service life time (scope)</param>
        public ApplicationServiceInfo(Type interfaceType, Type implementationType, ServiceLifetime lifetime)
        {
            InterfaceType = interfaceType ?? throw new ArgumentNullException(nameof(interfaceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime;

            if (!InterfaceType.IsAssignableFrom(ImplementationType)) throw new ArgumentException(
                $"Service implementation type {ImplementationType.FullName} is not " +
                $"assignable to the service interface {InterfaceType}.");
        }

        /// <summary>
        /// a type of the service (interface)
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// a service (interface) implementation
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// a service life time (scope)
        /// </summary>
        public ServiceLifetime Lifetime { get; }
    }
}

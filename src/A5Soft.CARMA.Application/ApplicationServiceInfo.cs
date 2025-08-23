using System;
using System.Reflection;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Basic info about an application service for DI.
    /// </summary>
    public class ApplicationServiceInfo
    {
        internal ApplicationServiceInfo(Type interfaceType, Type implementationType,
            ServiceLifetime lifetime, bool allowMultiple)
        {
            InterfaceType = interfaceType ?? throw new ArgumentNullException(nameof(interfaceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime;
            AllowMultiple = allowMultiple;
        }

        /// <summary>
        /// Creates a new <see cref="ApplicationServiceInfo"/> instance for the app service type specified.
        /// </summary>
        /// <param name="implementationType">the app service type to get info about</param>
        internal ApplicationServiceInfo(Type implementationType)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));

            var implementationAttr = implementationType
                .GetCustomAttribute<DefaultServiceImplementationAttribute>();
            if (null == implementationAttr) throw new InvalidOperationException(
                $"Type {implementationType.FullName} does not have a DefaultServiceImplementationAttribute.");

            InterfaceType = implementationAttr.ServiceInterfaceType;
            if (!InterfaceType.IsAssignableFrom(ImplementationType)) throw new ArgumentException(
                $"Service implementation type {ImplementationType.FullName} is not " +
                $"assignable to the service interface {InterfaceType}.");

            var interfaceAttr = implementationAttr.ServiceInterfaceType
                .GetCustomAttribute<ServiceAttribute>();
            if (null == interfaceAttr) throw new InvalidOperationException(
                $"{implementationType.FullName} declares that it implements interface " +
                $"{InterfaceType.FullName} however this interface does not have a ServiceAttribute.");

            Lifetime = interfaceAttr.Lifetime;
            AllowMultiple = interfaceAttr.AllowMultipleImplementations;
        }

        /// <summary>
        /// Creates a new <see cref="ApplicationServiceInfo"/> instance for the external service specified.
        /// </summary>
        /// <param name="externalService">an external service to get info about</param>
        internal ApplicationServiceInfo(IExternalServiceDescriptor externalService)
        {
            if (null == externalService) throw new ArgumentNullException(nameof(externalService));

            ImplementationType = externalService.ImplementationType ??
                throw new ArgumentException($"Implementation type cannot be null " +
                $"(external service: {externalService.GetType().FullName}).", nameof(externalService));

            InterfaceType = externalService.InterfaceType;
            if (null != InterfaceType && !InterfaceType.IsAssignableFrom(ImplementationType)) throw new ArgumentException(
                $"Service implementation type {ImplementationType.FullName} is not " +
                $"assignable to the service interface {InterfaceType}. (for external service: {externalService.GetType().FullName})");

            Lifetime = externalService.Lifetime;
            AllowMultiple = externalService.AllowMultipleImplementations;
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

        /// <summary>
        /// whether there could be multiple implementations of this application service
        /// </summary>
        public bool AllowMultiple { get; }


        /// <summary>
        /// Gets a value indicating whether this service is an implementation of the same service as the other one.
        /// </summary>
        /// <param name="otherService">the other service</param>
        /// <returns>a value indicating whether this service is an implementation of the same service as the other one</returns>
        public bool IsDuplicateFor(ApplicationServiceInfo otherService)
        {
            if (null == otherService) throw new ArgumentNullException(nameof(otherService));

            if (AllowMultiple) return false;

            if (null == InterfaceType && null == otherService.InterfaceType)
                return ImplementationType == otherService.ImplementationType;

            if (null == InterfaceType || null == otherService.InterfaceType)
                return false;

            return InterfaceType == otherService.InterfaceType;
        }
    }
}

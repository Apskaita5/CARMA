using System;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.IoC;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// Basic info about an application service for DI.
    /// </summary>
    public class ApplicationServiceInfo
    {
        internal ApplicationServiceInfo(Type interfaceType, Type implementationType, ServiceLifetime lifetime,
            bool allowMultiple, BuildConfiguration forBuildConfiguration)
        {
            InterfaceType = interfaceType;
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime;
            AllowMultiple = allowMultiple;
            ForBuildConfiguration = forBuildConfiguration;
            IsRemote = null != InterfaceType && InterfaceType.IsDefined(typeof(RemoteServiceAttribute), false);
            // in the constructor from an implementation type, after Lifetime is set
            IsTenantScoped = null != InterfaceType && InterfaceType.IsDefined(typeof(TenantScopedAttribute), false);
            if (IsTenantScoped && Lifetime == ServiceLifetime.Singleton) throw new InvalidOperationException(
                $"Service {InterfaceType.FullName} is a singleton and cannot be tenant scoped: singletons are app wide.");
        }

        internal ApplicationServiceInfo(Type interfaceType, object instance, ServiceLifetime lifetime,
            bool allowMultiple, BuildConfiguration forBuildConfiguration)
        {
            InterfaceType = interfaceType;
            ImplementationInstance = instance ?? throw new ArgumentNullException(nameof(instance));
            ImplementationType = instance.GetType();
            Lifetime = lifetime;
            AllowMultiple = allowMultiple;
            ForBuildConfiguration = forBuildConfiguration;
            IsRemote = null != InterfaceType && InterfaceType.IsDefined(typeof(RemoteServiceAttribute), false);
            // in the constructor from an implementation type, after Lifetime is set
            IsTenantScoped = null != InterfaceType && InterfaceType.IsDefined(typeof(TenantScopedAttribute), false);
            if (IsTenantScoped && Lifetime == ServiceLifetime.Singleton) throw new InvalidOperationException(
                $"Service {InterfaceType.FullName} is a singleton and cannot be tenant scoped: singletons are app wide.");
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

            var interfaceAttrs = implementationAttr.ServiceInterfaceType
                .GetCustomAttributes<ServiceAttribute>().ToList();
            if (interfaceAttrs.Count > 1) throw new InvalidOperationException(
                $"Interface {implementationAttr.ServiceInterfaceType.FullName} is marked with more than one service " +
                $"attribute ({string.Join(", ", interfaceAttrs.Select(a => a.GetType().Name))}): a use case is either " +
                "remote ([UseCase]) or local ([LocalUseCase]).");
            var interfaceAttr = interfaceAttrs.FirstOrDefault() ?? throw new InvalidOperationException(
                $"{implementationType.FullName} declares that it implements interface " +
                $"{InterfaceType.FullName} however this interface does not have a ServiceAttribute.");

            ForBuildConfiguration = implementationAttr.ForBuildConfiguration;
            Lifetime = interfaceAttr.Lifetime;
            AllowMultiple = interfaceAttr.AllowMultipleImplementations;
            IsRemote = null != InterfaceType && InterfaceType.IsDefined(typeof(RemoteServiceAttribute), false);
            // in the constructor from an implementation type, after Lifetime is set
            IsTenantScoped = null != InterfaceType && InterfaceType.IsDefined(typeof(TenantScopedAttribute), false);
            if (IsTenantScoped && Lifetime == ServiceLifetime.Singleton) throw new InvalidOperationException(
                $"Service {InterfaceType.FullName} is a singleton and cannot be tenant scoped: singletons are app wide.");
        }

        /// <summary>
        /// Creates a new <see cref="ApplicationServiceInfo"/> instance for the external service specified.
        /// </summary>
        /// <param name="externalService">an external service to get info about</param>
        internal ApplicationServiceInfo(ExternalServiceDescriptor externalService)
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
            ForBuildConfiguration = externalService.ForBuildConfiguration;
            IsRemote = false;
            IsTenantScoped = false;
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
        /// a ready service instance to register; null for a service the container creates
        /// from <see cref="ImplementationType"/>
        /// </summary>
        public object ImplementationInstance { get; }

        /// <summary>
        /// whether the service interface is a remote service (marked with <see cref="RemoteServiceAttribute"/>,
        /// e.g. a use case), invoked through the data portal when it runs remotely
        /// </summary>
        public bool IsRemote { get; }

        /// <summary>
        /// whether the service interface is marked with <see cref="TenantScopedAttribute"/>: resolved only in a scope
        /// with a tenant, and overridable per tenant
        /// </summary>
        public bool IsTenantScoped { get; }

        /// <summary>
        /// A build configuration that the service type is meant for.
        /// </summary>
        public BuildConfiguration ForBuildConfiguration { get; }


        /// <summary>The same implementation registered under its own type, e.g. as one of a tenant service's choices.</summary>
        internal ApplicationServiceInfo AsSelf()
            => new ApplicationServiceInfo(null, ImplementationType, Lifetime, false, ForBuildConfiguration);

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

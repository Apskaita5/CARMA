using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// The system services an IoC manager (app implementation) adds from the plugins loaded (<see cref="IoCManagerBase.AddSystemServices"/>).
    /// A system service is the only implementation of its interface: nothing (framework, app or plugin) may implement
    /// it as well, so no one can override it; and it is app wide, never tenant scoped.
    /// </summary>
    public sealed class SystemServiceCollection
    {
        private readonly List<ApplicationServiceInfo> _services = new List<ApplicationServiceInfo>();


        internal SystemServiceCollection() { }


        /// <summary>Gets the system services added.</summary>
        public IReadOnlyList<ApplicationServiceInfo> Services => _services;


        /// <summary>
        /// Adds a system service implemented by a type the container creates.
        /// </summary>
        /// <param name="interfaceType">the service interface</param>
        /// <param name="implementationType">a concrete class implementing it</param>
        /// <param name="lifetime">the lifetime; if the interface has a <see cref="ServiceAttribute"/>, it must be
        /// the lifetime declared there</param>
        /// <exception cref="ArgumentException">the implementation does not fit the interface, or the lifetime
        /// differs from the one declared</exception>
        /// <exception cref="InvalidOperationException">the interface is tenant scoped</exception>
        public SystemServiceCollection Add(Type interfaceType, Type implementationType, ServiceLifetime lifetime)
        {
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));
            if (null == implementationType) throw new ArgumentNullException(nameof(implementationType));
            if (!implementationType.IsClass || implementationType.IsAbstract || implementationType.ContainsGenericParameters)
                throw new ArgumentException($"{implementationType.FullName} is not a concrete class.", nameof(implementationType));
            if (!interfaceType.IsAssignableFrom(implementationType)) throw new ArgumentException(
                $"{implementationType.FullName} does not implement {interfaceType.FullName}.", nameof(implementationType));

            _services.Add(new ApplicationServiceInfo(interfaceType, implementationType, Check(interfaceType, lifetime),
                AllowsMultiple(interfaceType), BuildConfiguration.Any));
            return this;
        }

        /// <summary>
        /// Adds a system service implemented by a type the container creates.
        /// </summary>
        /// <typeparam name="TInterface">the service interface</typeparam>
        /// <typeparam name="TImplementation">a concrete class implementing it</typeparam>
        /// <param name="lifetime">the lifetime; if the interface has a <see cref="ServiceAttribute"/>, it must be
        /// the lifetime declared there</param>
        public SystemServiceCollection Add<TInterface, TImplementation>(ServiceLifetime lifetime)
            where TImplementation : class, TInterface
            => Add(typeof(TInterface), typeof(TImplementation), lifetime);

        /// <summary>
        /// Adds a system service as an instance created by the IoC manager: a singleton.
        /// </summary>
        /// <param name="interfaceType">the service interface</param>
        /// <param name="instance">the instance</param>
        /// <exception cref="ArgumentException">the instance does not implement the interface, or the interface
        /// declares another lifetime than singleton</exception>
        /// <exception cref="InvalidOperationException">the interface is tenant scoped</exception>
        public SystemServiceCollection AddInstance(Type interfaceType, object instance)
        {
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));
            if (null == instance) throw new ArgumentNullException(nameof(instance));
            if (!interfaceType.IsInstanceOfType(instance)) throw new ArgumentException(
                $"{instance.GetType().FullName} does not implement {interfaceType.FullName}.", nameof(instance));

            _services.Add(new ApplicationServiceInfo(interfaceType, instance,
                Check(interfaceType, ServiceLifetime.Singleton), AllowsMultiple(interfaceType), BuildConfiguration.Any));
            return this;
        }


        private static ServiceLifetime Check(Type interfaceType, ServiceLifetime lifetime)
        {
            if (interfaceType.IsDefined(typeof(TenantScopedAttribute), false)) throw new InvalidOperationException(
                $"{interfaceType.FullName} is tenant scoped: a system service is app wide.");

            var declared = interfaceType.GetCustomAttribute<ServiceAttribute>();
            if (null != declared && declared.Lifetime != lifetime) throw new ArgumentException(
                $"{interfaceType.FullName} is declared {declared.Lifetime}, not {lifetime}.", nameof(lifetime));

            return lifetime;
        }

        private static bool AllowsMultiple(Type interfaceType)
            => interfaceType.GetCustomAttribute<ServiceAttribute>()?.AllowMultipleImplementations ?? false;
    }
}

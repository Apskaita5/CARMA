using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.HealthChecks;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.IoC;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// What the framework looks for in an assembly, found in a single pass over its types and kept for
    /// the life of the assembly: service implementations, associated health checks, and external services (for DI),
    /// remote service interfaces (for the data portal), lookup types (for the generic lookup allowlist),
    /// configuration types (for the app configuration) and unmanaged assemblies (for loading).
    /// </summary>
    /// <remarks>
    /// <para>One scan serves <see cref="IoC"/>, <see cref="IoCManager"/>,
    /// <see cref="Configuration.Extensions.AddAppConfiguration(IAppConfigurationProvider, Assembly[])"/> and
    /// <see cref="DataPortal.DataPortalConventions"/>, so an assembly's types are enumerated once however
    /// many of them use it. An application with 10–20k use cases pays for it once.</para>
    /// <para>Scans are kept in a <see cref="ConditionalWeakTable{TKey,TValue}"/>, so a plugin assembly
    /// loaded into a collectible load context can still be unloaded.</para>
    /// </remarks>
    internal sealed class AssemblyScan
    {
        private static readonly ConditionalWeakTable<Assembly, AssemblyScan> Cache
            = new ConditionalWeakTable<Assembly, AssemblyScan>();


        private AssemblyScan(string source, IEnumerable<Type> types, bool forAssembly)
        {
            Source = source;

            var implementations = new List<ApplicationServiceInfo>();
            var healthChecks = new List<ApplicationServiceInfo>();
            var configurations = new List<ApplicationServiceInfo>();
            var remoteServices = new Dictionary<string, Type>(StringComparer.Ordinal);
            var trustedWireTypes = new Dictionary<string, Type>(StringComparer.Ordinal);
            IAssemblyDependencies dependencies = null;

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters) continue;

                ProcessDefaultServiceImplementation(type, implementations, remoteServices, trustedWireTypes, healthChecks);
                ProcessLookup(type, trustedWireTypes);
                if (IsDomainObject(type)) AddName(trustedWireTypes, type);
                ProcessConfiguration(type, configurations);

                if (forAssembly && typeof(IAssemblyDependencies).IsAssignableFrom(type))
                {
                    if (null != dependencies) throw new InvalidOperationException(
                        $"Asssembly {source} defines more than one IAssemblyDependencies implementation" +
                        $" (see {type.FullName} and {dependencies.GetType().FullName}).");
                    dependencies = GetAssemblyDependencies(type);
                }
            }

            ServiceImplementations = implementations;
            HealthChecks = healthChecks;
            ExternalDependencies = dependencies;
            RemoteServices = remoteServices;
            TrustedWireTypes = trustedWireTypes;
            Configurations = configurations;
        }


        /// <summary>Gets a description of what was scanned, for messages.</summary>
        public string Source { get; }

        /// <summary>Gets the concrete classes marked with <see cref="DefaultServiceImplementationAttribute"/>.</summary>
        public IReadOnlyList<ApplicationServiceInfo> ServiceImplementations { get; }

        /// <summary>Gets the concrete classes deriving directly from <see cref="ConfigurationBase"/>.</summary>
        public IReadOnlyList<ApplicationServiceInfo> Configurations { get; }

        /// <summary>Gets the concrete classes for <see cref="IHealthCheck"/>.</summary>
        public IReadOnlyList<ApplicationServiceInfo> HealthChecks { get; }

        /// <summary>Gets the project assembly dependencies if declared by a <see cref="IAssemblyDependencies"/> implementation.</summary>
        public IAssemblyDependencies ExternalDependencies { get; }

        /// <summary>
        /// Gets the remote service interfaces (marked with <see cref="RemoteServiceAttribute"/>, use cases included)
        /// implemented here, by their wire names. An interface without an implementation cannot be invoked, so it
        /// is not here.
        /// </summary>
        public IReadOnlyDictionary<string, Type> RemoteServices { get; }

        /// <summary>
        /// Gets the types found here that are trusted to move by wire, by their wire names: every implementation of
        /// a remote service, every concrete domain object (deriving from <see cref="DomainObject{T}"/>, entities
        /// included), every lookup type and its entity type (which may be declared in another assembly).
        /// See <see cref="DataPortal.RemoteUseCaseRegistry.TrustedWireTypes"/>.
        /// </summary>
        public IReadOnlyDictionary<string, Type> TrustedWireTypes { get; }


        /// <summary>
        /// Gets the scan of an assembly, scanning it on first use.
        /// </summary>
        /// <exception cref="InvalidOperationException">some types of the assembly cannot be loaded, e.g. a
        /// dependency is missing; the message lists the loader errors</exception>
        public static AssemblyScan Of(Assembly assembly)
        {
            if (null == assembly) throw new ArgumentNullException(nameof(assembly));

            return Cache.GetValue(assembly, a => new AssemblyScan(a.FullName, GetTypes(a), true));
        }

        /// <summary>
        /// Scans the types given, as if they were an assembly; not cached. For tests.
        /// </summary>
        internal static AssemblyScan Of(IEnumerable<Type> types, string source = "types")
            => new AssemblyScan(source, types ?? throw new ArgumentNullException(nameof(types)), false);


        /// <summary>
        /// Gets the services found, for the build configuration given: the default implementations, 
        /// associated health checks and the external services.
        /// </summary>
        public IEnumerable<ApplicationServiceInfo> GetServices(BuildConfiguration forBuildConfiguration)
        {
            foreach (var implementation in ServiceImplementations)
            {
                if (implementation.ForBuildConfiguration.Serves(forBuildConfiguration))
                    yield return implementation;
            }

            foreach (var healthCheck in HealthChecks)
            {
                if (healthCheck.ForBuildConfiguration.Serves(forBuildConfiguration))
                    yield return healthCheck;
            }

            if (null != ExternalDependencies?.ExternalServices)
            {
                foreach (var descriptor in ExternalDependencies.ExternalServices)
                {
                    if (descriptor.ForBuildConfiguration.Serves(forBuildConfiguration))
                        yield return new ApplicationServiceInfo(descriptor);
                }
            }
        }

        /// <summary>
        /// Gets the configuration types found for the build configuration given.
        /// </summary>
        public IEnumerable<ApplicationServiceInfo> GetConfigurations(BuildConfiguration forBuildConfiguration)
            => Configurations.Where(t => t.ForBuildConfiguration.Serves(forBuildConfiguration));


        /// <summary>
        /// Gets the entity type T of a class deriving from <see cref="LookupBase{T}"/>; null for any other type.
        /// </summary>
        internal static Type GetLookupEntityType(Type type)
        {
            for (var current = type; null != current && current != typeof(object); current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(LookupBase<>))
                    return current.GetGenericArguments()[0];
            }

            return null;
        }


        private void ProcessDefaultServiceImplementation(Type type,
            List<ApplicationServiceInfo> serviceImplementations, Dictionary<string, Type> remoteServices,
            Dictionary<string, Type> trustedWireTypes, List<ApplicationServiceInfo> healthChecks)
        {
            if (!type.IsDefined(typeof(DefaultServiceImplementationAttribute), false)) return;

            var serviceInfo = new ApplicationServiceInfo(type);
            serviceImplementations.Add(serviceInfo);

            if (serviceInfo.IsRemote)
            {
                // the interface routes calls; only the implementation may be named as data (a lookup requester)
                AddName(remoteServices, serviceInfo.InterfaceType);
                AddName(trustedWireTypes, type);
            }

            var healthCheckAttr = type.GetCustomAttribute<WithHealthCheckAttribute>();

            if (null == healthCheckAttr) return;

            healthChecks.Add(new ApplicationServiceInfo(typeof(IHealthCheck),
                healthCheckAttr.HealthCheckType, ServiceLifetime.Singleton, 
                true, serviceInfo.ForBuildConfiguration));
        }

        /// <summary>
        /// Gets a value indicating whether a type derives from <see cref="DomainObject{T}"/> (entities included).
        /// </summary>
        internal static bool IsDomainObject(Type type)
        {
            for (var current = type?.BaseType; null != current && current != typeof(object); current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(DomainObject<>)) return true;
            }

            return false;
        }

        private static void ProcessLookup(Type type, Dictionary<string, Type> trustedWireTypes)
        {
            var entityType = GetLookupEntityType(type);
            if (null == entityType) return;

            AddName(trustedWireTypes, type);
            AddName(trustedWireTypes, entityType);
        }

        /// <summary>
        /// Adds a type by its wire name; the same type again does nothing.
        /// </summary>
        /// <exception cref="InvalidOperationException">another type has the same wire name (possible only for
        /// entity types from two different assemblies)</exception>
        private static void AddName(Dictionary<string, Type> into, Type type)
        {
            var name = RemoteUseCaseDescriptor.GetName(type);
            if (into.TryGetValue(name, out var existing))
            {
                if (existing != type) throw new InvalidOperationException(
                    $"Types {existing.AssemblyQualifiedName} and {type.AssemblyQualifiedName} have the same name, " +
                    "so the data portal cannot tell them apart.");
                return;
            }

            into.Add(name, type);
        }


        private void ProcessConfiguration(Type type, List<ApplicationServiceInfo> configurations)
        {
            if (type.BaseType != typeof(ConfigurationBase)) return;

            ConfigurationBase configuration;
            try
            {
                configuration = (ConfigurationBase)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to instantiate ConfigurationBase" +
                    $" implementation of type {type.FullName}.", ex);
            }

            configurations.Add(new ApplicationServiceInfo(configuration.InterfaceType, type,
                ServiceLifetime.Singleton, false, configuration.ForBuildConfiguration));
        }

        private IAssemblyDependencies GetAssemblyDependencies(Type type)
        {
            try
            {
                return (IAssemblyDependencies)Activator.CreateInstance(type, true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to instantiate IAssemblyDependencies" +
                    $" implementation of type {type.FullName}.", ex);
            }
        }

        private static Type[] GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var errors = ex.LoaderExceptions.Where(e => null != e).Select(e => e.Message).Distinct().Take(10);
                throw new InvalidOperationException(
                    $"Assembly {assembly.FullName} cannot be scanned, some of its types cannot be loaded: " +
                    string.Join("; ", errors), ex);
            }
        }
    }
}

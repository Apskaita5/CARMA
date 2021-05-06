using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Extension methods for IoC containers.
    /// </summary>
    public static class IoC
    {

        /// <summary>
        /// Gets a list of default service implementations that are defined in the assemblies specified.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <param name="buildConfiguration">application build configuration (client/server)
        /// to get service implementations for</param>
        public static List<ApplicationServiceInfo> GetDefaultServices(BuildConfiguration buildConfiguration,
            params Assembly[] assemblies)
        {
            if (null == assemblies || assemblies.Length < 1) throw new ArgumentNullException(nameof(assemblies));
            if (buildConfiguration == BuildConfiguration.Any) throw new ArgumentException(
                "Cannot get service implementations for all app build configurations.");

            var result = new List<ApplicationServiceInfo>();

            foreach (var assembly in assemblies)
            {
                var implementations = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Select(t => (Implementation: t,
                        Attr: t.GetCustomAttribute<DefaultServiceImplementationAttribute>()))
                    .Where(i => null != i.Attr && i.Attr.ForBuildConfiguration.HasFlag(buildConfiguration))
                    .Select(v => new ApplicationServiceInfo(v.Attr.ServiceInterfaceType, 
                        v.Implementation, v.Attr.GetServiceLifetime(v.Implementation)));

                foreach (var implementation in implementations)
                {
                    var duplicate = result.FirstOrDefault(
                        s => s.InterfaceType == implementation.InterfaceType);
                    if (null != duplicate) throw new InvalidOperationException(
                            $"Service interface type {implementation.InterfaceType} " +
                            $"has multiple default implementations: {duplicate.ImplementationType.FullName} " +
                            $"and {implementation.ImplementationType.FullName}.");
                    
                    result.Add(implementation);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets default service implementations for CARMA framework.
        /// </summary>
        public static List<ApplicationServiceInfo> GetFrameworkServices()
        {
            return GetDefaultServices(BuildConfiguration.Client, 
                    typeof(IoC).Assembly, typeof(IDomainObject).Assembly)
                .ToList();
        }

    }
}

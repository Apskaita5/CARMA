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
        /// <param name="forBuildConfiguration">application build configuration (client/server)
        /// to get service implementations for</param>
        public static List<ApplicationServiceInfo> GetApplicationServices(BuildConfiguration forBuildConfiguration,
            params Assembly[] assemblies)
        {
            if (null == assemblies || assemblies.Length < 1) throw new ArgumentNullException(nameof(assemblies));
            if (forBuildConfiguration == BuildConfiguration.Any) throw new ArgumentException(
                "Cannot get service implementations for all app build configurations.");

            var allAppServices = new List<ApplicationServiceInfo>();
            foreach (var assembly in assemblies)
            {
                allAppServices.AddRange(assembly.GetApplicationServices(forBuildConfiguration));
                allAppServices.AddRange(assembly.GetExternalServices(forBuildConfiguration));
            }

            var result = new List<ApplicationServiceInfo>();
            foreach (var appService in allAppServices)
            {
                if (!appService.AllowMultiple)
                {
                    var duplicate = result.FirstOrDefault(
                        s => s.IsDuplicateFor(appService));
                    if (null != duplicate) throw new InvalidOperationException(
                        $"Service interface type {(appService.InterfaceType?.FullName ?? "[by implementation]")} " +
                        $"has multiple default implementations: {duplicate.ImplementationType.FullName} " +
                        $"and {appService.ImplementationType.FullName}.");
                }

                result.Add(appService);
            }

            // only add framework default services if they are not overriden/reimplemented in the application
            var frameworkServices = typeof(IoC).Assembly
                .GetApplicationServices(forBuildConfiguration)
                .ToList();
            frameworkServices.AddRange(typeof(IDomainObject).Assembly.GetApplicationServices(forBuildConfiguration));
            foreach (var frameworkService in frameworkServices)
            {
                if (!result.Any(s => s.InterfaceType == frameworkService.InterfaceType))
                    result.Add(frameworkService);
            }

            return result;
        }

        private static IEnumerable<ApplicationServiceInfo> GetApplicationServices(this Assembly assembly,
            BuildConfiguration forBuildConfiguration)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => (Implementation: t, Attr: t.GetCustomAttribute<DefaultServiceImplementationAttribute>()))
                .Where(i => null != i.Attr && i.Attr.ForBuildConfiguration.HasFlag(forBuildConfiguration))
                .Select(v => new ApplicationServiceInfo(v.Implementation));
        }

        private static IEnumerable<ApplicationServiceInfo> GetExternalServices(this Assembly assembly,
            BuildConfiguration forBuildConfiguration)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && (typeof(IExternalServiceDescriptor).IsAssignableFrom(t)))
                .Select(t => (IExternalServiceDescriptor)Activator.CreateInstance(t,true))
                .Where(d => d.ForBuildConfiguration.HasFlag(forBuildConfiguration))
                .Select(v => new ApplicationServiceInfo(v));
        }
    }
}

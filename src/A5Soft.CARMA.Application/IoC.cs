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
        /// Gets a dictionary of default service implementations that are
        /// defined in the assemblies specified. (key is a service interface type)
        /// </summary>
        /// <param name="assemblies"></param>
        public static Dictionary<Type, (Type Implementation, ServiceLifetime Lifetime)> GetDefaultServices(
            params Assembly[] assemblies)
        {
            if (null == assemblies || assemblies.Length < 1) throw new ArgumentNullException(nameof(assemblies));

            var result = new Dictionary<Type, (Type Implementation, ServiceLifetime Lifetime)>();

            foreach (var assembly in assemblies)
            {
                var implementations = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract)
                    .Select(t => (Implementation: t,
                        Attr: t.GetCustomAttribute<DefaultServiceImplementationAttribute>()))
                    .Where(i => null != i.Attr);
                foreach (var implementation in implementations)
                {
                    if (result.ContainsKey(implementation.Attr.ServiceInterfaceType)) 
                        throw new InvalidOperationException(
                            $"Service interface type {implementation.Attr.ServiceInterfaceType.FullName} " +
                            $"has multiple default implementations: {result[implementation.Attr.ServiceInterfaceType].Implementation.FullName} " +
                            $"and {implementation.Implementation.FullName}.");
                    
                    result.Add(implementation.Attr.ServiceInterfaceType, (implementation.Implementation,
                        implementation.Attr.GetServiceLifetime(implementation.Implementation)));
                }
            }

            return result;
        }

        /// <summary>
        /// Appends default service implementations for CARMA framework (if there are no
        /// default implementations at the application level).
        /// </summary>
        /// <param name="dic"></param>
        public static void AppendFrameworkImplementations(
            this Dictionary<Type, (Type Implementation, ServiceLifetime Lifetime)> dic)
        {
            if (null == dic) throw new ArgumentNullException(nameof(dic));

            var frameworkImplementations = GetDefaultServices(
                typeof(IoC).Assembly, typeof(IDomainObject).Assembly);

            foreach (var frameworkImplementation in frameworkImplementations)
            {
                if (!dic.ContainsKey(frameworkImplementation.Key)) dic.Add(
                    frameworkImplementation.Key, frameworkImplementation.Value);
            }
        }

    }
}

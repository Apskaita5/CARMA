using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Used to designate a default (remote) service or use case (interface) implementation
    /// that should be added to IoC container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultServiceImplementationAttribute : Attribute
    {
        /// <summary>
        /// add the attribute
        /// </summary>
        /// <param name="serviceInterfaceType">a type of the service interface implemented by the class</param>
        /// <param name="forBuildConfiguration">a configuration of the app build (client/server/any)
        /// that the implementation is meant for</param>
        public DefaultServiceImplementationAttribute(Type serviceInterfaceType,
            BuildConfiguration forBuildConfiguration = BuildConfiguration.Any)
        {
            ServiceInterfaceType = serviceInterfaceType ??
                throw new ArgumentNullException(nameof(serviceInterfaceType));
            ForBuildConfiguration = forBuildConfiguration;

            if (!serviceInterfaceType.IsInterface) throw new ArgumentException(
                $"Service interface type should be an interface while {serviceInterfaceType.FullName} is not.",
                nameof(serviceInterfaceType));
        }


        /// <summary>
        /// a type of the service interface implemented by the class
        /// </summary>
        public Type ServiceInterfaceType { get; }

        /// <summary>
        /// a configuration of the app build (client/server/any) that the implementation is meant for
        /// </summary>
        public BuildConfiguration ForBuildConfiguration { get; }
    }
}

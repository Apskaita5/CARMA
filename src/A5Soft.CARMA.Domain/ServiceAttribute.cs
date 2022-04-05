using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Used to designate a service interface that should be added to IoC container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// Default constructor for ServiceAttribute.
        /// </summary>
        /// <param name="lifetime">a lifetime of the service within an IoC container</param>
        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            Lifetime = lifetime;
        }

        /// <summary>
        /// Gets a lifetime of the use case within a IoC container. Default is Transient.
        /// </summary>
        public ServiceLifetime Lifetime { get; }
    }
}

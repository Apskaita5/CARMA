using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Used to designate a service interface that should be added to IoC container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// Default constructor for ServiceAttribute.
        /// </summary>
        /// <param name="lifetime">a lifetime of the service within an IoC container</param>
        /// <param name="allowMultipleImplementations">whether multiple implementations of the service are allowed (not only a single)</param>
        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient, bool allowMultipleImplementations = false)
        {
            Lifetime = lifetime;
            AllowMultipleImplementations = allowMultipleImplementations;
        }

        /// <summary>
        /// Gets a lifetime of the use case within a IoC container. Default is Transient.
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Gets a value indicating whether multiple implementations of the service are allowed (not only a single).
        /// Default is false.
        /// </summary>
        public bool AllowMultipleImplementations { get; }
    }
}

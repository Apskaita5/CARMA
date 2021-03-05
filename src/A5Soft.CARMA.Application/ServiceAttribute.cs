using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Used to designate a service interface that should be added to IoC container.
    /// Use cases have a derived attribute <see cref="UseCaseAttribute"/> .
    /// Remote services (that support remote invoke) have a derived attribute <see cref="RemoteServiceAttribute"/> .
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ServiceAttribute : Attribute
    {

        /// <summary>
        /// Default constructor for ServiceAttribute.
        /// </summary>
        /// <param name="lifetime">a lifetime of the service within an IoC container</param>
        /// <param name="defaultImplementation">a default implementation of the interface (if any) to
        /// use in IoC container unless specified otherwise</param>
        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient,
            Type defaultImplementation = null)
        {
            Lifetime = lifetime;
            DefaultImplementation = defaultImplementation;

            if (null != defaultImplementation)
            {
                if (!defaultImplementation.IsClass || defaultImplementation.IsAbstract
                    || !defaultImplementation.IsPublic) throw new ArgumentException(
                    $"Service implementation should be a public non abstract class, " +
                    $"while type {defaultImplementation.FullName} is not.",
                    nameof(defaultImplementation));
            }
        }


        /// <summary>
        /// Gets a lifetime of the use case within a IoC container. Default is Transient.
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Gets default implementation of the interface (if any) to
        /// use in IoC container unless specified otherwise.
        /// </summary>
        public Type DefaultImplementation { get; }

    }
}

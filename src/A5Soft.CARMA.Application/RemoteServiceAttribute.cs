using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Used to designate a service (use case or other) interface that has a remote method.
    /// There could only be one remote method per remote service interface.
    /// A service implementation can only have one remote interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class RemoteServiceAttribute : ServiceAttribute
    {

        /// <summary>
        /// Default constructor for RemoteServiceAttribute.
        /// </summary>
        /// <param name="lifetime">a lifetime of the service within an IoC container</param>
        public RemoteServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient) 
            : base(lifetime) { }

    }
}

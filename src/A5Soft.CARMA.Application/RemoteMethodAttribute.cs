using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Used to designate a service (use case or other) interface method that should be invoked remotely.
    /// There could only be one remote method per remote service interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RemoteMethodAttribute : Attribute
    {
    }
}

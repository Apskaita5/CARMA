using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.DataPortal.EndPoints
{
    /// <summary>
    /// The data portal transports by name, and a proxy per endpoint. Holds no user state: the endpoint of a scope
    /// is <see cref="IDataPortalConnection"/>.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IDataPortalTransports
    {
        /// <summary>Gets the names of the transports available (framework, app and plugins).</summary>
        IReadOnlyCollection<string> Names { get; }

        /// <summary>Gets the proxy for an endpoint, created on first use and kept.</summary>
        /// <exception cref="ArgumentException">an unknown transport, or an address missing or not expected</exception>
        IDataPortalProxy GetProxy(DataPortalEndpoint endpoint);
    }
}

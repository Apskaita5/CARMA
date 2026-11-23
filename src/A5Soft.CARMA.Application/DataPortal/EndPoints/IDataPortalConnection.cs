using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.DataPortal.EndPoints
{
    /// <summary>
    /// The data portal endpoint of the current scope, fixed when the scope is created: by the app for each scope
    /// it creates (e.g. from the server and the transport chosen at login), or, if the app sets none, the
    /// configured one (<see cref="IDataPortalConnectionConfiguration"/>). Changing server creates a new scope, as
    /// changing tenant does; the session ends, as an identity belongs to one server.
    /// </summary>
    [Service(ServiceLifetime.Scoped)]
    public interface IDataPortalConnection
    {
        /// <summary>Gets the endpoint of this scope.</summary>
        /// <exception cref="InvalidOperationException">none set, and none configured</exception>
        DataPortalEndpoint Endpoint { get; }

        /// <summary>Gets whether the calls go to a server (false for the local transport).</summary>
        /// <exception cref="InvalidOperationException">no endpoint set, and none configured</exception>
        bool IsRemote { get; }

        /// <summary>Gets the proxy for this scope's endpoint.</summary>
        /// <exception cref="InvalidOperationException">no endpoint set, and none configured</exception>
        IDataPortalProxy Proxy { get; }

        /// <summary>Sets the endpoint of this scope, once, before it is used.</summary>
        /// <exception cref="InvalidOperationException">already set, or already used</exception>
        /// <exception cref="ArgumentException">an unknown transport, or an address missing or not expected</exception>
        void Initialize(DataPortalEndpoint endpoint);
    }
}

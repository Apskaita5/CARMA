using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.DataPortal.EndPoints
{
    /// <summary>
    /// The data portal endpoint of the current scope: the endpoint of the scope's identity
    /// (<see cref="Authorization.CarmaIdentity.Endpoint"/>, e.g. chosen at login), or, if it has none, the configured one
    /// (<see cref="Client.IDataPortalConnectionConfiguration"/>), or else the local one in a build that runs the backend in
    /// process. Changing server starts a new scope with a new identity, as an identity belongs to one server.
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
        /// <exception cref="ArgumentException">an unknown transport, or an address missing or not expected</exception>
        IDataPortalProxy Proxy { get; }
    }
}

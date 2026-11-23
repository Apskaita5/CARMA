using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.DataPortal.EndPoints
{
    /// <summary>
    /// A kind of data portal connection: creates the proxy for an endpoint. Found by the scan: the framework has
    /// <see cref="LocalDataPortalTransport"/> and <c>HttpDataPortalTransport</c>; an app or a plugin adds another by
    /// implementing this interface (with its own <see cref="IDataPortalProxy"/>). Names must be unique.
    /// </summary>
    [Service(ServiceLifetime.Singleton, true)]
    public interface IDataPortalTransport
    {
        /// <summary>Gets the transport name the endpoints refer to (unique, compared ignoring case).</summary>
        string Name { get; }

        /// <summary>Gets whether an endpoint of this transport needs a server address.</summary>
        bool RequiresAddress { get; }

        /// <summary>
        /// Creates a proxy for an endpoint of this transport. Called once per endpoint; the proxy is kept for the
        /// life of the app and shared by every user and scope, so it must hold no per-user state.
        /// </summary>
        /// <param name="endpoint">an endpoint of this transport, its address checked against
        /// <see cref="RequiresAddress"/></param>
        IDataPortalProxy CreateProxy(DataPortalEndpoint endpoint);
    }
}

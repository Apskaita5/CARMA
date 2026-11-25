using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.DataPortal.EndPoints;
using A5Soft.CARMA.Application.IoC;
using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// The default <see cref="IDataPortalConnection"/>: the endpoint of the scope's identity; without one, the
    /// configured endpoint; without that, in a build that runs the backend in process (a server, an ASP.NET server with a
    /// web UI, a thick client), the local one: its use cases run in process.
    /// </summary>
    [DefaultServiceImplementation(typeof(IDataPortalConnection))]
    public sealed class DataPortalConnection : IDataPortalConnection
    {
        private readonly IDataPortalTransports _transports;
        private readonly IDataPortalConnectionConfiguration _configuration;
        private readonly IBuildInfo _build;
        private readonly IAuthenticationStateProvider _authentication;


        /// <summary>
        /// Creates a new connection.
        /// </summary>
        /// <param name="transports">the transports</param>
        /// <param name="configuration">the configured endpoint, used if the app sets none</param>
        /// <param name="build">the app build, deciding whether the local endpoint is implied</param>
        /// <param name="authentication">the scope's identity, holding the endpoint chosen for the session</param>
        public DataPortalConnection(IDataPortalTransports transports, IDataPortalConnectionConfiguration configuration,
            IBuildInfo build, IAuthenticationStateProvider authentication)
        {
            _transports = transports ?? throw new ArgumentNullException(nameof(transports));
            _configuration = configuration;
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
        }


        /// <inheritdoc />
        public DataPortalEndpoint Endpoint => _authentication.Identity.Endpoint
            ?? _configuration?.Endpoint
            ?? (_build.IncludesBackend ? new DataPortalEndpoint(DataPortalEndpoint.Local) : null)
            ?? throw new InvalidOperationException("No data portal endpoint is set for this scope's identity, and none is configured.");

        /// <inheritdoc />
        public IDataPortalProxy Proxy => _transports.GetProxy(Endpoint);

        /// <inheritdoc />
        public bool IsRemote => Proxy.IsRemote;
    }
}

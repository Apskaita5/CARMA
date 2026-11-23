using A5Soft.CARMA.Application.DataPortal.EndPoints;
using A5Soft.CARMA.Application.IoC;
using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// The default <see cref="IDataPortalConnection"/>: set once per scope. Without an endpoint set or configured, a
    /// build that runs the backend in process (a server, an ASP.NET server with a web UI, a thick client) uses the
    /// local one: its use cases run in process.
    /// </summary>
    [DefaultServiceImplementation(typeof(IDataPortalConnection))]
    public sealed class DataPortalConnection : IDataPortalConnection
    {
        private readonly IDataPortalTransports _transports;
        private readonly IDataPortalConnectionConfiguration _configuration;
        private readonly IBuildInfo _build;
        private DataPortalEndpoint _endpoint;


        /// <summary>
        /// Creates a new connection.
        /// </summary>
        /// <param name="transports">the transports</param>
        /// <param name="configuration">the configured endpoint, used if the app sets none</param>
        /// <param name="build">the app build, deciding whether the local endpoint is implied</param>
        public DataPortalConnection(IDataPortalTransports transports, IDataPortalConnectionConfiguration configuration,
            IBuildInfo build)
        {
            _transports = transports ?? throw new ArgumentNullException(nameof(transports));
            _configuration = configuration;
            _build = build ?? throw new ArgumentNullException(nameof(build));
        }


        /// <inheritdoc />
        public DataPortalEndpoint Endpoint => _endpoint ?? (_endpoint = _configuration?.Endpoint
            ?? (_build.IncludesBackend ? new DataPortalEndpoint(DataPortalEndpoint.Local) : null)
            ?? throw new InvalidOperationException(
                "No data portal endpoint is set for this scope, and none is configured."));

        /// <inheritdoc />
        public IDataPortalProxy Proxy => _transports.GetProxy(Endpoint);

        /// <inheritdoc />
        public bool IsRemote => Proxy.IsRemote;


        /// <inheritdoc />
        public void Initialize(DataPortalEndpoint endpoint)
        {
            if (null == endpoint) throw new ArgumentNullException(nameof(endpoint));
            if (null != _endpoint) throw new InvalidOperationException(
                "The data portal endpoint of a scope cannot change: create a new scope for another server.");

            _transports.GetProxy(endpoint);   // checks the transport and the address now, not at the first call
            _endpoint = endpoint;
        }
    }
}

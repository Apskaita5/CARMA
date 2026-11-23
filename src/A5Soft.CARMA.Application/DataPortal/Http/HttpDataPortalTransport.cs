using System;
using A5Soft.CARMA.Application.DataPortal.Client;
using A5Soft.CARMA.Application.DataPortal.EndPoints;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.DataPortal.Http
{
    /// <summary>
    /// The HTTP transport: an endpoint's address is the absolute URL of the server's data portal endpoint.
    /// </summary>
    [DefaultServiceImplementation(typeof(IDataPortalTransport), BuildConfiguration.Client)]
    public sealed class HttpDataPortalTransport : IDataPortalTransport
    {
        private readonly IDataPortalClientOptions _options;
        private readonly IHttpDataPortalClientOptions _httpOptions;


        /// <summary>
        /// Creates the HTTP transport.
        /// </summary>
        /// <param name="options">the client data portal options</param>
        /// <param name="httpOptions">the client HTTP transport options</param>
        public HttpDataPortalTransport(IDataPortalClientOptions options, IHttpDataPortalClientOptions httpOptions)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpOptions = httpOptions ?? throw new ArgumentNullException(nameof(httpOptions));
        }


        /// <inheritdoc />
        public string Name => DataPortalEndpoint.Http;

        /// <inheritdoc />
        public bool RequiresAddress => true;

        /// <inheritdoc />
        public IDataPortalProxy CreateProxy(DataPortalEndpoint endpoint)
        {
            if (null == endpoint) throw new ArgumentNullException(nameof(endpoint));
            return new HttpDataPortalProxy(endpoint.Address, _options, _httpOptions);
        }
    }
}

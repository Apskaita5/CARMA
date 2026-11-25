using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.DataPortal.Server;
using A5Soft.CARMA.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The server side of the data portal: runs the calls an endpoint receives. Implemented by
    /// <see cref="Server.ServerDataPortal"/>.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IServerDataPortal
    {
        /// <summary>Gets the server options.</summary>
        IDataPortalServerOptions Options { get; }

        /// <summary>
        /// Gets the public settings of the server, for an endpoint to publish to the clients.
        /// </summary>
        ServerSettings GetServerSettings();

        /// <summary>
        /// Runs a call: checks it, finds the caller's session, authorizes and invokes the use case.
        /// </summary>
        /// <param name="call">the call header</param>
        /// <param name="body">the request body: the parameters and the files</param>
        /// <param name="services">a service provider of the request scope</param>
        /// <param name="cancellationToken">a token to cancel the call</param>
        /// <returns>the response: a result or an error; never throws for a failed call</returns>
        Task<DataPortalResponse> HandleAsync(DataPortalCall call, IDataPortalRequestBody body,
            IServiceProvider services, CancellationToken cancellationToken);

        /// <summary>
        /// Creates the error response for a failure of the endpoint itself (e.g. a call it could not read).
        /// </summary>
        /// <param name="exception">the failure</param>
        /// <param name="refusedBeforeBody">whether the call was refused before any of its data was read</param>
        DataPortalResponse CreateErrorResponse(Exception exception, bool refusedBeforeBody);
    }
}

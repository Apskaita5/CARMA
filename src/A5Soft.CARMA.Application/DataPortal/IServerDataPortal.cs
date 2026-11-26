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
        /// Gets the public settings of the server, for an endpoint to publish to the clients: the identity
        /// of the application and the limits every call must respect. Public to anyone who can reach the
        /// endpoint, without authentication.
        /// </summary>
        /// <returns>a new instance, which the caller may change</returns>
        ServerSettings GetServerSettings();

        /// <summary>
        /// Runs a call: checks it, finds the caller's session, authorizes and invokes the use case.
        /// </summary>
        /// <param name="call">the description of the call, as received; untrusted</param>
        /// <param name="body">the request body (the parameters and the files),
        /// read only once the call has been cleared</param>
        /// <param name="services">a service provider of the request scope</param>
        /// <param name="cancellationToken">a token signalled when the caller goes away (http.RequestAborted)</param>
        /// <returns>a response for the endpoint to send; the endpoint disposes it once it is sent;
        /// never throws for a failed call, a failure is returned as an error response</returns>
        Task<DataPortalResponse> HandleAsync(DataPortalCall call, IDataPortalRequestBody body,
            IServiceProvider services, CancellationToken cancellationToken);

        /// <summary>
        /// Creates the error response for a failure of the endpoint itself (e.g. a call it could not read).
        /// Creates an error response for a failure, mapped, logged and stripped the same way as the
        /// failures of <see cref="HandleAsync"/>.
        /// </summary>
        /// <param name="exception">the failure; a <see cref="RemoteCallRejectedException"/> for a refusal</param>
        /// <param name="refusedBeforeBody">whether the call was refused before any of its data was read</param>
        /// <param name="cancellationToken">endpoints cancellation token used to verify the cancellation reason
        /// (pass http.RequestAborted)</param>
        DataPortalResponse CreateErrorResponse(Exception exception, bool refusedBeforeBody,
            CancellationToken cancellationToken);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The data of an incoming call, read by the server data portal on demand: the payload first,
    /// then the files in order. Implemented by a server endpoint over its transport.
    /// </summary>
    /// <remarks>
    /// <para>The server data portal reads nothing before it has authorized the call, and may not
    /// read the data at all if it refuses the call.</para>
    /// <para>An implementation reports malformed or missing data by throwing a
    /// <see cref="RemoteCallRejectedException"/> of kind <see cref="RemoteErrorKind.BadRequest"/>.</para>
    /// </remarks>
    public interface IDataPortalRequestBody
    {
        /// <summary>
        /// Reads the payload: the parameters as UTF-8 JSON.
        /// </summary>
        /// <param name="maxLength">a max length of the payload in bytes</param>
        /// <param name="cancellationToken">a token to cancel the read</param>
        /// <returns>the payload</returns>
        /// <exception cref="RemoteCallRejectedException">the payload is missing, malformed or
        /// longer than allowed</exception>
        Task<ReadOnlyMemory<byte>> ReadPayloadAsync(int maxLength, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the start of the next file; the previous file must have been read to its end.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the read</param>
        /// <returns>the next file; null when the call carries no more data</returns>
        /// <exception cref="RemoteCallRejectedException">the data is malformed</exception>
        Task<IncomingFile> ReadNextFileAsync(CancellationToken cancellationToken);
    }
}

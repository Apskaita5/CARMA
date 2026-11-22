using System;
using System.Collections.Generic;
using A5Soft.CARMA.Application.DataPortal.Internal;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// An outgoing remote use case call in the canonical, transport independent form, as the client
    /// data portal hands it to an <see cref="IDataPortalProxy"/>.
    /// </summary>
    /// <remarks>A proxy must deliver <see cref="Call"/> before the data (<see cref="Payload"/>, then
    /// <see cref="Files"/> in order), so that the server can refuse the call before the files are
    /// sent.</remarks>
    public sealed class DataPortalRequest
    {
        private readonly SendTracker _tracker;


        internal DataPortalRequest(DataPortalCall call, ReadOnlyMemory<byte> payload,
            IReadOnlyList<DataPortalFile> files, SendTracker tracker)
        {
            Call = call ?? throw new ArgumentNullException(nameof(call));
            Payload = payload;
            Files = files ?? throw new ArgumentNullException(nameof(files));
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        }


        /// <summary>Gets the description of the call.</summary>
        public DataPortalCall Call { get; }

        /// <summary>
        /// Gets the parameters as UTF-8 JSON: an array with one value per parameter, in signature
        /// order, every file replaced by a reference <c>{"fileRef": n}</c>.
        /// </summary>///
        /// <remarks>
        /// <see cref="ReadOnlyMemory{T}"/> over the buffer the JSON was written to, so that the payload
        /// (up to <c>MaxPayloadLength</c>) is never copied just to have an array of the exact length; and
        /// read-only, so that a proxy cannot change what it sends. Valid until the call has been sent.
        /// A proxy that needs an array gets it without copying with
        /// <see cref="System.Runtime.InteropServices.MemoryMarshal.TryGetArray{T}"/>, which always succeeds
        /// for the memory the client data portal creates.
        /// </remarks>
        public ReadOnlyMemory<byte> Payload { get; }

        /// <summary>
        /// Gets the files the payload references, in reference order (file n is <c>{"fileRef": n}</c>).
        /// </summary>
        public IReadOnlyList<DataPortalFile> Files { get; }

        /// <summary>
        /// Gets a value indicating whether any byte of any file has been read. A call can be sent
        /// again only while it is false: the files are read once.
        /// </summary>
        public bool FilesRead => _tracker.AnyRead;


        /// <summary>Gets the limit violation found while the files were read, if any.</summary>
        internal string LimitViolation => _tracker.Violation;
    }
}

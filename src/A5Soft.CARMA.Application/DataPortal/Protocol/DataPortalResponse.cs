using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using A5Soft.CARMA.Domain.Files;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// The outcome of a remote use case call in the canonical, transport independent form: created by
    /// the server data portal for an endpoint to send, and by a proxy from what it has received.
    /// </summary>
    /// <remarks>
    /// <para>A response owns its resources: on the server the uploaded files and a downloaded file, on
    /// the client the transport connection behind a downloaded file. Dispose it once it has been
    /// handled; take the file first (<see cref="TakeFile"/>) to keep it.</para>
    /// <para>A refreshed identity accompanies any result except an error.</para>
    /// </remarks>
    public sealed class DataPortalResponse : IDisposable
    {
        private readonly IReadOnlyList<IDisposable> _resources;
        private FileContent _file;
        private int _disposed;


        private DataPortalResponse(DataPortalResultKind kind, string resultType, ReadOnlyMemory<byte> value,
            FileContent file, RemoteError error, IdentityEnvelope refreshedIdentity, bool refusedBeforeBody,
            IEnumerable<IDisposable> resources)
        {
            Kind = kind;
            ResultType = resultType;
            Value = value;
            _file = file;
            Error = error;
            RefreshedIdentity = refreshedIdentity;
            RefusedBeforeBody = refusedBeforeBody;
            _resources = resources?.Where(r => null != r).ToList() ?? (IReadOnlyList<IDisposable>)Array.Empty<IDisposable>();
        }


        /// <summary>Gets what the response holds.</summary>
        public DataPortalResultKind Kind { get; }

        /// <summary>
        /// Gets the wire type name of the result value, for <see cref="DataPortalResultKind.Value"/>.
        /// </summary>
        public string ResultType { get; }

        /// <summary>
        /// Gets the result value as UTF-8 JSON, for <see cref="DataPortalResultKind.Value"/>.
        /// A domain object is its DTO; a file nested in it is always null.
        /// </summary>
        public ReadOnlyMemory<byte> Value { get; }

        /// <summary>
        /// Gets the downloaded file, for <see cref="DataPortalResultKind.File"/>; null after
        /// <see cref="TakeFile"/>.
        /// </summary>
        public FileContent File => _file;

        /// <summary>Gets the error, for <see cref="DataPortalResultKind.Error"/>.</summary>
        public RemoteError Error { get; }

        /// <summary>
        /// Gets the caller's refreshed identity; null if it has not changed. Never set for an error.
        /// </summary>
        public IdentityEnvelope RefreshedIdentity { get; }

        /// <summary>
        /// Gets a value indicating whether the call was refused before any of its data was read.
        /// A transport that lets a client wait for acceptance before sending the data (HTTP
        /// <c>Expect: 100-continue</c>) uses it to tell the client not to send the files.
        /// </summary>
        public bool RefusedBeforeBody { get; }


        /// <summary>Creates a response with a result value.</summary>
        /// <param name="resultType">the wire type name of the result</param>
        /// <param name="value">the result as UTF-8 JSON</param>
        /// <param name="refreshedIdentity">a refreshed identity; null if unchanged</param>
        /// <param name="resources">resources the response owns</param>
        public static DataPortalResponse ForValue(string resultType, ReadOnlyMemory<byte> value,
            IdentityEnvelope refreshedIdentity, IEnumerable<IDisposable> resources = null)
        {
            if (string.IsNullOrEmpty(resultType)) throw new ArgumentNullException(nameof(resultType));

            return new DataPortalResponse(DataPortalResultKind.Value, resultType, value, null, null,
                refreshedIdentity, false, resources);
        }

        /// <summary>Creates a response without a result.</summary>
        /// <param name="refreshedIdentity">a refreshed identity; null if unchanged</param>
        /// <param name="resources">resources the response owns</param>
        public static DataPortalResponse ForNone(IdentityEnvelope refreshedIdentity,
            IEnumerable<IDisposable> resources = null)
            => new DataPortalResponse(DataPortalResultKind.None, null, default, null, null,
                refreshedIdentity, false, resources);

        /// <summary>Creates a response with a downloaded file, which the response owns.</summary>
        /// <param name="file">the file</param>
        /// <param name="refreshedIdentity">a refreshed identity; null if unchanged</param>
        /// <param name="resources">other resources the response owns</param>
        public static DataPortalResponse ForFile(FileContent file, IdentityEnvelope refreshedIdentity,
            IEnumerable<IDisposable> resources = null)
            => new DataPortalResponse(DataPortalResultKind.File, null, default,
                file ?? throw new ArgumentNullException(nameof(file)), null, refreshedIdentity, false, resources);

        /// <summary>Creates an error response.</summary>
        /// <param name="error">the error</param>
        /// <param name="refusedBeforeBody">whether the call was refused before any of its data was read</param>
        public static DataPortalResponse ForError(RemoteError error, bool refusedBeforeBody)
            => new DataPortalResponse(DataPortalResultKind.Error, null, default, null,
                error ?? throw new ArgumentNullException(nameof(error)), null, refusedBeforeBody, null);


        /// <summary>
        /// Takes the downloaded file over: the caller owns it from now on, and disposing the
        /// response no longer disposes it.
        /// </summary>
        /// <returns>the file; null if the response holds none, or it has been taken already</returns>
        public FileContent TakeFile() => Interlocked.Exchange(ref _file, null);

        /// <summary>
        /// Releases the resources the response owns, and the file unless it has been taken.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

            foreach (var resource in new IDisposable[] { TakeFile() }.Concat(_resources))
            {
                try
                {
                    resource?.Dispose();
                }
                catch (Exception)
                {
                    // releasing one resource must not keep the rest from being released
                }
            }
        }
    }
}

using System.Collections.Generic;

namespace A5Soft.CARMA.Application.DataPortal.Http
{
    /// <summary>
    /// Names and values of the headers that carry a data portal call. Everything that describes
    /// a call travels in headers; the request body is a multipart body (the JSON payload, then the
    /// uploaded files), and the response body is an optional identity block followed by the result.
    /// </summary>
    /// <remarks>
    /// <para>Every portal outcome, an error included, is answered with HTTP 200 and described by
    /// <see cref="Result"/>. The only exception is 417 Expectation Failed for a call with files
    /// refused before its body was read, which the HTTP proxy turns back into a normal response.
    /// Any other status code comes from the infrastructure.</para>
    /// <para>A GET to the endpoint, with no custom header, is answered with the JSON of the
    /// <see cref="ServerSettings"/> and the <see cref="Protocol"/> header.</para>
    /// <para>Every header value is ASCII: free text is percent-encoded UTF-8.</para>
    /// <para>A browser client on another origin needs CORS that allows <see cref="RequestHeaders"/>
    /// and <c>Authorization</c>, and exposes <see cref="ResponseHeaders"/>; otherwise the browser
    /// hides the response headers and every call looks like a technical failure.</para>
    /// </remarks>
    public static class DataPortalHeaders
    {
        /// <summary>
        /// The version of the protocol this implementation speaks. A server rejects a call of any
        /// other version, so a stale client gets a clear "bad request" rather than a misread call.
        /// </summary>
        public const string CurrentProtocolVersion = "2";

        /// <summary>
        /// The scheme of the <c>Authorization</c> header value: <c>Carma &lt;token&gt;</c>.
        /// </summary>
        public const string AuthorizationScheme = "Carma";

        /// <summary>
        /// Request: the protocol version, <see cref="CurrentProtocolVersion"/>. Response to the settings
        /// GET: the protocol version of the server, which the client compares with its own.
        /// </summary>
        public const string Protocol = "X-Carma-Protocol";

        /// <summary>Request: the name of the use case interface to invoke.</summary>
        public const string UseCase = "X-Carma-Use-Case";

        /// <summary>
        /// Request: the wire type names of the parameters, in the order of the remote method
        /// signature, comma separated.
        /// </summary>
        public const string ParameterTypes = "X-Carma-Parameter-Types";

        /// <summary>
        /// Request, generic remote methods only: the wire names of the generic arguments, comma separated.
        /// Resolved on the server through its trusted wire types only. Absent for a method that is not generic.
        /// </summary>
        public const string GenericArguments = "X-Carma-Generic-Arguments";

        /// <summary>
        /// Request: the client's plugin fingerprint (<see cref="IoC.PluginCatalog.Fingerprint"/>), on every call; a
        /// call from a client with other plugins is refused.
        /// </summary>
        public const string Plugins = "X-Carma-Plugins";

        /// <summary>Request: the name of the caller's culture.</summary>
        public const string Culture = "X-Carma-Culture";

        /// <summary>Request: the name of the caller's UI culture.</summary>
        public const string UICulture = "X-Carma-UI-Culture";

        /// <summary>
        /// Request: the number of file parts that follow the payload part. Lets the server refuse
        /// too many files before reading any of them.
        /// </summary>
        public const string FileCount = "X-Carma-File-Count";

        /// <summary>The name of the multipart part that holds the payload; always the first part.</summary>
        public const string PayloadPartName = "payload";

        /// <summary>The prefix of the names of the multipart parts that hold the files.</summary>
        public const string FilePartNamePrefix = "file-";

        /// <summary>Gets the name of the multipart part that holds file n.</summary>
        /// <param name="index">the index of the file, as its reference <c>{"fileRef": n}</c> gives it</param>
        public static string FilePartName(int index)
            => FilePartNamePrefix + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>Response: what the body holds, one of the <see cref="ResultKinds"/>.</summary>
        public const string Result = "X-Carma-Result";

        /// <summary>Response: the wire type name of the result value.</summary>
        public const string ResultType = "X-Carma-Result-Type";

        /// <summary>
        /// Response: the length in bytes of the refreshed identity (JSON) at the start of the body;
        /// absent when the identity has not changed. The identity travels in the body because a full
        /// identity can exceed the response header limits of reverse proxies.
        /// </summary>
        public const string IdentityLength = "X-Carma-Identity-Length";

        /// <summary>Response: the name of the downloaded file.</summary>
        public const string FileName = "X-Carma-File-Name";

        /// <summary>Response: the media (MIME) type of the downloaded file.</summary>
        public const string FileContentType = "X-Carma-File-Content-Type";

        /// <summary>
        /// Response: the length of the downloaded file in bytes, when it is known in advance.
        /// It excludes the identity block, so it differs from the length of the body.
        /// </summary>
        public const string FileLength = "X-Carma-File-Length";


        /// <summary>
        /// The values of the <see cref="Result"/> header.
        /// </summary>
        public static class ResultKinds
        {
            /// <summary>After the identity block, the body is the JSON of the result value.</summary>
            public const string Value = "value";

            /// <summary>There is no result: the method returns nothing, or it has returned null.
            /// The body holds the identity block only, if any.</summary>
            public const string None = "none";

            /// <summary>After the identity block, the body is the content of a downloaded file.</summary>
            public const string File = "file";

            /// <summary>The body is the JSON of a <see cref="RemoteError"/>.</summary>
            public const string Error = "error";
        }


        /// <summary>
        /// Gets the custom headers a client sends, for a CORS policy (allowed headers);
        /// <c>Authorization</c> must be allowed as well.
        /// </summary>
        public static IReadOnlyList<string> RequestHeaders { get; } = new[]
        {
            Protocol, UseCase, ParameterTypes, GenericArguments, Plugins, Culture, UICulture, FileCount
        };

        /// <summary>
        /// Gets the headers a server sends, for a CORS policy (exposed headers).
        /// </summary>
        public static IReadOnlyList<string> ResponseHeaders { get; } = new[]
        {
            Protocol, Result, ResultType, IdentityLength, FileName, FileContentType, FileLength
        };
    }
}

using A5Soft.CARMA.Application.DataPortal.Server;
using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.DataPortal.Http
{
    /// <summary>
    /// The options of the HTTP endpoint of the server data portal, read from the <c>HttpDataPortalServerOptions</c>
    /// section of the app configuration. A setting missing from the configuration keeps its default.
    /// </summary>
    /// <remarks>
    /// <code>
    /// "HttpDataPortalServerOptions": {
    ///   "MaxRequestHeaderLength": 8192,
    ///   "MaxPartHeadersLength": 16384
    /// }
    /// </code>
    /// </remarks>
    public class HttpDataPortalServerOptions : ConfigurationBase, IHttpDataPortalServerOptions
    {
        /// <summary>
        /// The bytes of framing allowed for each multipart part (boundary, part headers), on top of the
        /// payload and the files, when the endpoint sets the host's request body limit.
        /// </summary>
        public const int MaxFramingPerPart = 4096;

        private const int KB = 1024;


        /// <inheritdoc />
        public override Type InterfaceType => typeof(IHttpDataPortalServerOptions);

        /// <inheritdoc />
        public override BuildConfiguration ForBuildConfiguration => BuildConfiguration.Server;


        /// <inheritdoc />
        /// <remarks>4 KB to 64 KB; at least the client's limit.</remarks>
        public int MaxRequestHeaderLength { get; set; } = 8 * KB;

        /// <inheritdoc />
        /// <remarks>1 KB to 64 KB. A part's headers hold its name, file name (twice, for non-ASCII names),
        /// media type and length.</remarks>
        public int MaxPartHeadersLength { get; set; } = 16 * KB;


        /// <summary>
        /// Gets the max length of a request body that the server data portal options allow: the payload,
        /// the files, and the framing of every part. The endpoint sets the host's body limit to it.
        /// </summary>
        /// <param name="options">the server data portal options</param>
        public static long GetMaxRequestLength(IDataPortalServerOptions options)
        {
            if (null == options) throw new ArgumentNullException(nameof(options));

            return options.MaxPayloadLength + options.MaxUploadLength
                + (options.MaxFileCount + 1L) * MaxFramingPerPart;
        }

        /// <summary>
        /// Checks every setting, so that a misconfiguration stops the app at startup.
        /// </summary>
        /// <exception cref="InvalidOperationException">a setting is out of range; the message lists every
        /// problem found</exception>
        public override void Validate()
        {
            base.Validate();

            var problems = new List<string>();

            if (MaxRequestHeaderLength < 4 * KB || MaxRequestHeaderLength > 64 * KB)
                problems.Add($"{nameof(MaxRequestHeaderLength)} must be between 4096 and 65536, it is {MaxRequestHeaderLength}");
            if (MaxPartHeadersLength < KB || MaxPartHeadersLength > 64 * KB)
                problems.Add($"{nameof(MaxPartHeadersLength)} must be between 1024 and 65536, it is {MaxPartHeadersLength}");

            if (problems.Count > 0) throw new InvalidOperationException(
                $"The {nameof(HttpDataPortalServerOptions)} configuration is not valid: {string.Join("; ", problems)}.");
        }
    }
}

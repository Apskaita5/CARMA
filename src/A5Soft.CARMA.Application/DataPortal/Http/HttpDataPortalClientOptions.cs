using System;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Application.DataPortal.Internal;

namespace A5Soft.CARMA.Application.DataPortal.Http
{
    /// <summary>
    /// The options of the HTTP transport on the client, read from the <c>HttpDataPortalClientOptions</c> section of
    /// the app configuration. A setting missing from the configuration keeps its default.
    /// </summary>
    /// <remarks>
    /// <para>Bound to <see cref="IHttpDataPortalClientOptions"/> by the startup configuration scan:</para>
    /// <code>
    /// "HttpDataPortalClientOptions": {
    ///   "MaxRequestHeaderLength": 8192,
    ///   "MaxRequestHeadersLength": 16384,
    ///   "BrowserRequestStreaming": false,
    ///   "CallTimeout": "00:01:40"
    /// }
    /// </code>
    /// </remarks>
    public class HttpDataPortalClientOptions : ConfigurationBase, IHttpDataPortalClientOptions
    {
        /// <inheritdoc />
        public override Type InterfaceType => typeof(IHttpDataPortalClientOptions);

        /// <inheritdoc />
        public override BuildConfiguration ForBuildConfiguration => BuildConfiguration.Client;


        /// <inheritdoc />
        /// <remarks>4 KB to 64 KB.</remarks>
        public int MaxRequestHeaderLength { get; set; } = (int)(8 * OptionsValidation.KB);

        /// <inheritdoc />
        /// <remarks>At least <see cref="MaxRequestHeaderLength"/>, at most 256 KB.</remarks>
        public int MaxRequestHeadersLength { get; set; } = (int)(16 * OptionsValidation.KB);

        /// <inheritdoc />
        public bool BrowserRequestStreaming { get; set; }

        /// <inheritdoc />
        public TimeSpan CallTimeout { get; set; } = TimeSpan.FromSeconds(100);


        /// <summary>
        /// Checks every setting, so that a misconfiguration stops the app at startup.
        /// </summary>
        /// <exception cref="InvalidOperationException">a setting is out of range; the message lists every
        /// problem found</exception>
        public override void Validate()
        {
            base.Validate();

            var v = new OptionsValidation(nameof(HttpDataPortalClientOptions));

            v.Range(MaxRequestHeaderLength, 4 * OptionsValidation.KB, 64 * OptionsValidation.KB, nameof(MaxRequestHeaderLength));
            v.Range(MaxRequestHeadersLength, 4 * OptionsValidation.KB, 256 * OptionsValidation.KB, nameof(MaxRequestHeadersLength));
            v.Require(CallTimeout > TimeSpan.Zero || CallTimeout == System.Threading.Timeout.InfiniteTimeSpan,
                $"{nameof(CallTimeout)} must be positive, or infinite (-00:00:00.001)");
            v.Require(MaxRequestHeadersLength >= MaxRequestHeaderLength,
                $"{nameof(MaxRequestHeadersLength)} must be at least {nameof(MaxRequestHeaderLength)}");

            v.ThrowIfInvalid();
        }
    }
}

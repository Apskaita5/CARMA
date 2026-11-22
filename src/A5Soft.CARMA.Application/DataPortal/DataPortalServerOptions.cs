using System;
using System.IO;
using A5Soft.CARMA.Application.DataPortal.Internal;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The options of the server data portal, read from the <c>DataPortal:Server</c> section of the app
    /// configuration. A setting missing from the configuration keeps its default.
    /// </summary>
    /// <remarks>
    /// <para>Bound to <see cref="IDataPortalServerOptions"/> by the startup configuration scan; consumers
    /// depend on the interface. Every property is a plain settable value:</para>
    /// <code>
    /// "DataPortal": {
    ///   "Server": {
    ///     "MaxFileCount": 50,
    ///     "MaxFileLength": 5242880,
    ///     "MaxUploadLength": 262144000,
    ///     "MaxPayloadLength": 4194304,
    ///     "MaxDepth": 32,
    ///     "CopyBufferSize": 81920,
    ///     "SpoolDirectory": "App_Data/upload-spool"
    ///   }
    /// }
    /// </code>
    /// <para>The limits are published to the clients in the <see cref="ServerSettings"/>, so they are
    /// never configured on a client.</para>
    /// </remarks>
    public class DataPortalServerOptions : ConfigurationBase, IDataPortalServerOptions
    {
        /// <summary>The smallest <see cref="MaxDepth"/> allowed: a DTO graph with a child list, lookups and
        /// nested values already takes about 10 levels.</summary>
        public const int MinMaxDepth = 16;

        /// <summary>The largest <see cref="MaxDepth"/> allowed: deeper JSON no longer protects against a stack
        /// overflow.</summary>
        public const int MaxMaxDepth = 128;


        /// <inheritdoc />
        public override Type InterfaceType => typeof(IDataPortalServerOptions);

        /// <inheritdoc />
        public override string RootSection => string.Empty;


        /// <inheritdoc />
        /// <remarks>0 to 1,000; 0 disables uploads.</remarks>
        public int MaxFileCount { get; set; } = 50;

        /// <inheritdoc />
        /// <remarks>64 KB to 2 GB.</remarks>
        public long MaxFileLength { get; set; } = 5 * OptionsValidation.MB;

        /// <inheritdoc />
        /// <remarks>At least <see cref="MaxFileLength"/>, at most 16 GB.</remarks>
        public long MaxUploadLength { get; set; } = 250 * OptionsValidation.MB;

        /// <inheritdoc />
        /// <remarks>256 KB to 256 MB.</remarks>
        public int MaxPayloadLength { get; set; } = (int)(4 * OptionsValidation.MB);

        /// <inheritdoc />
        /// <remarks>16 to 128.</remarks>
        public int MaxDepth { get; set; } = 32;

        /// <inheritdoc />
        /// <remarks>4 KB to 1 MB; keep it under 85,000 bytes, so that the buffers stay off the large object
        /// heap.</remarks>
        public int CopyBufferSize { get; set; } = 81920;

        /// <inheritdoc />
        /// <remarks>A relative path is resolved against the app path when the options are initialized. The
        /// directory must exist and be writable.</remarks>
        public string SpoolDirectory { get; set; }


        /// <summary>
        /// Resolves a relative <see cref="SpoolDirectory"/> against the app path.
        /// </summary>
        /// <param name="appPath">the path of the app</param>
        /// <param name="isDevelopment">whether the app runs in the development environment</param>
        public override void Init(string appPath, bool isDevelopment)
        {
            base.Init(appPath, isDevelopment);

            SpoolDirectory = string.IsNullOrWhiteSpace(SpoolDirectory) ? string.Empty : SpoolDirectory.Trim();

            if (null != SpoolDirectory && !Path.IsPathRooted(SpoolDirectory) && !string.IsNullOrWhiteSpace(appPath)
                && SpoolDirectory.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                SpoolDirectory = Path.GetFullPath(Path.Combine(appPath, SpoolDirectory));
        }

        /// <summary>
        /// Checks every setting, so that a misconfiguration stops the app at startup rather than failing the
        /// first call.
        /// </summary>
        /// <exception cref="InvalidOperationException">a setting is out of range, or the spool directory does
        /// not exist; the message lists every problem found</exception>
        public override void Validate()
        {
            base.Validate();

            var v = new OptionsValidation(nameof(DataPortalServerOptions));

            v.Range(MaxFileCount, 0, 1000, nameof(MaxFileCount));
            v.Range(MaxFileLength, 64 * OptionsValidation.KB, 2 * OptionsValidation.GB, nameof(MaxFileLength));
            v.Range(MaxUploadLength, 64 * OptionsValidation.KB, 16 * OptionsValidation.GB, nameof(MaxUploadLength));
            v.Require(MaxUploadLength >= MaxFileLength,
                $"{nameof(MaxUploadLength)} must be at least {nameof(MaxFileLength)}, or the file limit can never be reached");
            v.Range(MaxPayloadLength, 256 * OptionsValidation.KB, 256 * OptionsValidation.MB, nameof(MaxPayloadLength));
            v.Range(MaxDepth, MinMaxDepth, MaxMaxDepth, nameof(MaxDepth));
            v.Range(CopyBufferSize, 4 * OptionsValidation.KB, OptionsValidation.MB, nameof(CopyBufferSize));

            if (null != SpoolDirectory)
            {
                var valid = SpoolDirectory.IndexOfAny(Path.GetInvalidPathChars()) < 0;
                v.Require(valid, $"{nameof(SpoolDirectory)} is not a valid path: {SpoolDirectory}");
                if (valid) v.Require(Directory.Exists(SpoolDirectory),
                    $"{nameof(SpoolDirectory)} does not exist: {SpoolDirectory}");
            }

            v.ThrowIfInvalid();
        }
    }
}

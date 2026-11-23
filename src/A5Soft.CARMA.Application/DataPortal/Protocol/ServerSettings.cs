using A5Soft.CARMA.Application.DataPortal.Server;
using A5Soft.CARMA.Application.IoC.Plugins;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// The public settings of a server, which every client fetches before its first call: the identity of
    /// the application, and the limits every call must respect.
    /// </summary>
    /// <remarks>
    /// <para>A fixed framework type: the framework decides what is safe to publish. Anyone who can reach the
    /// server can read it, before logging in: the client needs it before its first call, and logging in is a
    /// call.</para>
    /// <para>Built by <see cref="ServerDataPortalBase.GetServerSettings"/> from the
    /// <see cref="IDataPortalServerOptions"/> and the <see cref="IAppVersionProvider"/>; received by the
    /// client through <see cref="IDataPortalProxy.GetServerSettingsAsync"/>. On the client every value is
    /// data received over the wire, checked before use.</para>
    /// </remarks>
    public sealed class ServerSettings
    {
        /// <summary>Gets or sets the name of the server's application.</summary>
        public string AppName { get; set; }

        /// <summary>Gets or sets the version of the server's application.</summary>
        public Version AppVersion { get; set; }

        /// <inheritdoc cref="IDataPortalServerOptions.MaxFileCount"/>
        public int MaxFileCount { get; set; }

        /// <inheritdoc cref="IDataPortalServerOptions.MaxFileLength"/>
        public long MaxFileLength { get; set; }

        /// <inheritdoc cref="IDataPortalServerOptions.MaxUploadLength"/>
        public long MaxUploadLength { get; set; }

        /// <inheritdoc cref="IDataPortalServerOptions.MaxPayloadLength"/>
        public int MaxPayloadLength { get; set; }

        /// <inheritdoc cref="IDataPortalServerOptions.MaxDepth"/>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Gets or sets the fingerprint of the plugins installed on the server
        /// (<see cref="IoC.PluginCatalog.Fingerprint"/>); the client refuses a server with other plugins.
        /// </summary>
        public string PluginFingerprint { get; set; }


        /// <summary>
        /// Creates the settings of the server.
        /// </summary>
        /// <param name="options">the server data portal options</param>
        /// <param name="versionProvider">the application's identity</param>/// <param name="plugins">the plugins installed on the server</param>
        internal static ServerSettings Create(IDataPortalServerOptions options, IAppVersionProvider versionProvider,
            PluginCatalog plugins)
        {
            if (null == options) throw new ArgumentNullException(nameof(options));
            if (null == versionProvider) throw new ArgumentNullException(nameof(versionProvider));
            if (null == plugins) throw new ArgumentNullException(nameof(plugins));

            return new ServerSettings
            {
                AppName = versionProvider.AppName,
                AppVersion = versionProvider.AppVersion,
                PluginFingerprint = plugins.Fingerprint,
                MaxFileCount = options.MaxFileCount,
                MaxFileLength = options.MaxFileLength,
                MaxUploadLength = options.MaxUploadLength,
                MaxPayloadLength = options.MaxPayloadLength,
                MaxDepth = options.MaxDepth
            };
        }

        /// <summary>
        /// Gets the problems of settings received from a server; empty if they are usable.
        /// </summary>
        internal IReadOnlyList<string> GetProblems()
        {
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(AppName)) problems.Add("no application name");
            if (null == AppVersion) problems.Add("no application version");
            if (MaxFileCount < 0) problems.Add("a negative file count limit");
            if (MaxFileLength < 1 || MaxUploadLength < 1 || MaxPayloadLength < 1) problems.Add("a non-positive size limit");
            if (MaxDepth < DataPortalServerOptions.MinMaxDepth || MaxDepth > DataPortalServerOptions.MaxMaxDepth)
                problems.Add($"a JSON depth limit of {MaxDepth}");
            if (string.IsNullOrWhiteSpace(PluginFingerprint)) problems.Add("no plugin fingerprint");

            return problems;
        }
    }
}

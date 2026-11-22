using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The details of a <see cref="RemoteErrorKind.VersionMismatch"/>, held in
    /// <see cref="RemoteError.Data"/>; read them with <c>error.Data.ToObject&lt;VersionMismatchData&gt;()</c>.
    /// </summary>
    public sealed class VersionMismatchData
    {
        /// <summary>Gets or sets the name of the client's application.</summary>
        public string ClientAppName { get; set; }

        /// <summary>Gets or sets the version of the client's application.</summary>
        public Version ClientAppVersion { get; set; }

        /// <summary>Gets or sets the name of the server's application; null if the server could not tell.</summary>
        public string ServerAppName { get; set; }

        /// <summary>Gets or sets the version of the server's application; null if the server could not tell.</summary>
        public Version ServerAppVersion { get; set; }

        /// <summary>Gets or sets how strictly the versions were compared.</summary>
        public AppVersionMatch AppVersionMatch { get; set; }

        /// <summary>Gets or sets a description of the mismatch, for the log; not localized.</summary>
        public string Reason { get; set; }

        /// <summary>Gets or sets the client's plugin fingerprint.</summary>
        public string ClientPluginFingerprint { get; set; }

        /// <summary>Gets or sets the server's plugin fingerprint.</summary>
        public string ServerPluginFingerprint { get; set; }
    }
}

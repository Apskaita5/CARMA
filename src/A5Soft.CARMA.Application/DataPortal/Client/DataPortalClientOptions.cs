using System;
using A5Soft.CARMA.Application.DataPortal.Internal;

namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// The options of the client data portal, read from the <c>DataPortalClientOptions</c> section of the app
    /// configuration (the class name, see <see cref="ConfigurationBase.RootSection"/>). A setting missing from the
    /// configuration keeps its default, so a client usually needs no configuration at all.
    /// </summary>
    /// <remarks>
    /// <para>Bound to <see cref="IDataPortalClientOptions"/> by the startup configuration scan; consumers
    /// depend on the interface. Every property is a plain settable value (the values below are the defaults):</para>
    /// <code>
    /// "DataPortalClientOptions": {
    ///   "AppVersionMatch": "MajorMinor",
    ///   "MaxIdentityLength": 1048576,
    ///   "MaxResponseLength": 67108864,
    ///   "CopyBufferSize": 81920
    /// }
    /// </code>
    /// <para>The limits a call must respect are not configured here: they come from the server, in the
    /// <see cref="Protocol.ServerSettings"/>.</para>
    /// </remarks>
    public class DataPortalClientOptions : ConfigurationBase, IDataPortalClientOptions
    {
        /// <inheritdoc />
        public override Type InterfaceType => typeof(IDataPortalClientOptions);


        /// <inheritdoc />
        public AppVersionMatch AppVersionMatch { get; set; } = AppVersionMatch.MajorMinor;

        /// <inheritdoc />
        /// <remarks>64 KB to 16 MB; an identity with 400 permissions takes about 16 KB.</remarks>
        public int MaxIdentityLength { get; set; } = (int)OptionsValidation.MB;

        /// <inheritdoc />
        /// <remarks>1 MB to 1 GB. Should be at least the server's payload limit: a save use case returns
        /// what it has received.</remarks>
        public int MaxResponseLength { get; set; } = (int)(64 * OptionsValidation.MB);

        /// <inheritdoc />
        /// <remarks>4 KB to 1 MB; keep it under 85,000 bytes, so that the buffers stay off the large object
        /// heap.</remarks>
        public int CopyBufferSize { get; set; } = 81920;


        /// <summary>
        /// Checks every setting, so that a misconfiguration stops the app at startup rather than failing the
        /// first call.
        /// </summary>
        /// <exception cref="InvalidOperationException">a setting is out of range; the message lists every
        /// problem found</exception>
        public override void Validate()
        {
            base.Validate();

            var v = new OptionsValidation(nameof(DataPortalClientOptions));

            v.Require(Enum.IsDefined(typeof(AppVersionMatch), AppVersionMatch),
                $"{nameof(AppVersionMatch)} must be one of {string.Join(", ", Enum.GetNames(typeof(AppVersionMatch)))}");
            v.Range(MaxIdentityLength, 64 * OptionsValidation.KB, 16 * OptionsValidation.MB, nameof(MaxIdentityLength));
            v.Range(MaxResponseLength, OptionsValidation.MB, OptionsValidation.GB, nameof(MaxResponseLength));
            v.Range(CopyBufferSize, 4 * OptionsValidation.KB, OptionsValidation.MB, nameof(CopyBufferSize));

            v.ThrowIfInvalid();
        }
    }
}

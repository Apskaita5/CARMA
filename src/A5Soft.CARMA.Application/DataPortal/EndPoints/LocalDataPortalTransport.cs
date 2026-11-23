using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.DataPortal.EndPoints
{
    /// <summary>
    /// The local transport: no transport at all. Its proxy is not remote, so the use cases run in process, which
    /// needs their server side services in the same process (a standalone or two-tier app).
    /// </summary>
    [DefaultServiceImplementation(typeof(IDataPortalTransport), BuildConfiguration.Backend)]
    public sealed class LocalDataPortalTransport : IDataPortalTransport
    {
        private static readonly LocalDataPortalProxy Proxy = new LocalDataPortalProxy();


        /// <inheritdoc />
        public string Name => DataPortalEndpoint.Local;

        /// <inheritdoc />
        public bool RequiresAddress => false;

        /// <inheritdoc />
        public IDataPortalProxy CreateProxy(DataPortalEndpoint endpoint) => Proxy;
    }
}

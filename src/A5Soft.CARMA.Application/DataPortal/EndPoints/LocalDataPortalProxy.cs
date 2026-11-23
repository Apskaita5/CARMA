using A5Soft.CARMA.Application.DataPortal.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal.EndPoints
{
    /// <summary>
    /// The null transport: use cases run in process, so the data portal is never called. Created by
    /// <see cref="LocalDataPortalTransport"/>.
    /// </summary>
    public sealed class LocalDataPortalProxy : IDataPortalProxy
    {
        /// <inheritdoc />
        public bool IsRemote => false;

        /// <inheritdoc />
        public Task<DataPortalResponse> SendAsync(DataPortalRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(
                "The data portal is not remote; use cases should run in process.");

        /// <inheritdoc />
        public Task<ServerSettings> GetServerSettingsAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(
                "The data portal is not remote; there is no server to get the settings of.");
    }
}

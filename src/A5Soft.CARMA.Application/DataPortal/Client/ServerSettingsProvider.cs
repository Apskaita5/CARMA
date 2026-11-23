using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.DataPortal.EndPoints;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// The default <see cref="IServerSettingsProvider"/>: fetches the settings of each server through its proxy
    /// once, shares one fetch among concurrent callers, and keeps the result until <see cref="Invalidate"/>.
    /// A failed fetch is not kept.
    /// </summary>
    [DefaultServiceImplementation(typeof(IServerSettingsProvider))]
    public class ServerSettingsProvider : IServerSettingsProvider
    {
        private readonly IDataPortalTransports _transports;
        private readonly object _lock = new object();
        private readonly Dictionary<DataPortalEndpoint, Task<ServerSettings>> _fetches
            = new Dictionary<DataPortalEndpoint, Task<ServerSettings>>();


        /// <param name="transports">the transports, giving the proxy of each endpoint</param>
        public ServerSettingsProvider(IDataPortalTransports transports)
        {
            _transports = transports ?? throw new ArgumentNullException(nameof(transports));
        }


        /// <inheritdoc />
        public Task<ServerSettings> GetAsync(DataPortalEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            if (null == endpoint) throw new ArgumentNullException(nameof(endpoint));

            var proxy = _transports.GetProxy(endpoint);
            if (!proxy.IsRemote) throw new InvalidOperationException(
                $"Endpoint {endpoint} is local: there is no server to get the settings of.");

            Task<ServerSettings> fetch;
            lock (_lock)
            {
                if (!_fetches.TryGetValue(endpoint, out fetch) || fetch.IsFaulted || fetch.IsCanceled)
                    _fetches[endpoint] = fetch = FetchAsync(proxy);
            }

            if (fetch.IsCompleted || !cancellationToken.CanBeCanceled) return fetch;

            return WaitAsync(fetch, cancellationToken);
        }

        /// <inheritdoc />
        public void Invalidate(DataPortalEndpoint endpoint)
        {
            if (null == endpoint) throw new ArgumentNullException(nameof(endpoint));

            lock (_lock)
            {
                // a fetch in progress is left to the callers waiting for it
                if (_fetches.TryGetValue(endpoint, out var fetch) && fetch.IsCompleted) _fetches.Remove(endpoint);
            }
        }


        private static async Task<ServerSettings> FetchAsync(IDataPortalProxy proxy)
        {
            // not cancelled by any one caller: others may be waiting for it
            await Task.Yield();

            var settings = await proxy.GetServerSettingsAsync(CancellationToken.None)
                .ConfigureAwait(false)
                ?? throw new DataPortalTransportException(RemoteErrorKind.BadRequest,
                "The proxy has returned no server settings.");

            var problems = settings.GetProblems();
            if (problems.Count > 0) throw new DataPortalTransportException(RemoteErrorKind.BadRequest,
                $"The server settings are not usable ({string.Join(", ", problems)}); " +
                "the client and the server are probably of different versions.");

            return settings;
        }

        private static async Task<ServerSettings> WaitAsync(Task<ServerSettings> fetch,
            CancellationToken cancellationToken)
        {
            var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
            {
                if (fetch != await Task.WhenAny(fetch, cancelled.Task).ConfigureAwait(false))
                    throw new OperationCanceledException(cancellationToken);
            }

            return await fetch.ConfigureAwait(false);
        }
    }
}

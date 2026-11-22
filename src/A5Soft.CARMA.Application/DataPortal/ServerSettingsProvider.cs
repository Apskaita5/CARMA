using System;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The default <see cref="IServerSettingsProvider"/>: fetches the settings through the
    /// <see cref="IDataPortalProxy"/> once, shares one fetch among concurrent callers, and keeps the
    /// result until <see cref="Invalidate"/>. A failed fetch is not kept.
    /// </summary>
    [DefaultServiceImplementation(typeof(IServerSettingsProvider))]
    public class ServerSettingsProvider : IServerSettingsProvider
    {
        private readonly IDataPortalProxy _proxy;
        private readonly object _lock = new object();
        private Task<ServerSettings> _fetch;


        /// <summary>
        /// Creates a new provider.
        /// </summary>
        /// <param name="proxy">the transport to fetch the settings through</param>
        public ServerSettingsProvider(IDataPortalProxy proxy)
        {
            _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
        }


        /// <inheritdoc />
        public Task<ServerSettings> GetAsync(CancellationToken cancellationToken = default)
        {
            Task<ServerSettings> fetch;
            lock (_lock)
            {
                if (null == _fetch || _fetch.IsFaulted || _fetch.IsCanceled) _fetch = FetchAsync();
                fetch = _fetch;
            }

            if (fetch.IsCompleted || !cancellationToken.CanBeCanceled) return fetch;

            return WaitAsync(fetch, cancellationToken);
        }

        /// <inheritdoc />
        public void Invalidate()
        {
            lock (_lock)
            {
                // a fetch in progress is left to the callers waiting for it
                if (null != _fetch && _fetch.IsCompleted) _fetch = null;
            }
        }


        private async Task<ServerSettings> FetchAsync()
        {
            // not cancelled by any one caller: others may be waiting for it
            await Task.Yield();

            var settings = await _proxy.GetServerSettingsAsync(CancellationToken.None).ConfigureAwait(false)
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

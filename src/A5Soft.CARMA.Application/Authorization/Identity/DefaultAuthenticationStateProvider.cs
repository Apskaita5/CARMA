using A5Soft.CARMA.Application.Authorization.Sessions;
using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Authorization.Identity
{
    /// <summary>
    /// The framework's identity holder of a scope (see <see cref="IAuthenticationStateProvider"/>).
    /// </summary>
    /// <remarks>Its optional dependencies decide what it does: with an <see cref="IIdentityStore"/> and an
    /// <see cref="IIdentityProvider"/> (builds with the server role) it checks the ticket of a local use case call; with
    /// an <see cref="IUiDispatcher"/> (WinForms, WPF) it runs the change handlers on the UI thread.</remarks>
    [DefaultServiceImplementation(typeof(IAuthenticationStateProvider))]
    public class DefaultAuthenticationStateProvider : IAuthenticationStateProvider, IDisposable
    {
        private readonly object _lock = new object();
        private readonly List<Func<IdentityChangedEventArgs, Task>> _handlers = new List<Func<IdentityChangedEventArgs, Task>>();
        private readonly ILogger _logger;
        private readonly IUiDispatcher _dispatcher;
        private readonly IIdentityStore _store;
        private readonly IIdentityProvider _identityProvider;
        private CarmaIdentity _identity = CarmaIdentity.CreateAnonymous();
        private bool _initialized;
        private bool _verified;


        /// <summary>Creates the provider.</summary>
        /// <param name="logger">a logger for failing handlers</param>
        /// <param name="dispatcher">the UI dispatcher; null if the app has none</param>
        /// <param name="identityStore">the identity store; null in builds without the server role</param>
        /// <param name="identityProvider">the app's identity provider; null in builds without the server role</param>
        public DefaultAuthenticationStateProvider(ILogger logger = null, IUiDispatcher dispatcher = null,
            IIdentityStore identityStore = null, IIdentityProvider identityProvider = null)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _store = identityStore;
            _identityProvider = identityProvider;
        }


        /// <inheritdoc />
        public CarmaIdentity Identity
        {
            get { lock (_lock) return _identity; }
        }


        /// <inheritdoc />
        public virtual Task InitializeAsync(CarmaIdentity identity, bool verified = false,
            CancellationToken cancellationToken = default)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));

            lock (_lock)
            {
                if (_initialized) throw new InvalidOperationException(
                    "The identity of a scope is set once: start a new scope for another session.");
                _identity = identity;
                _verified = verified;
                _initialized = true;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual async Task EnsureCurrentAsync(CancellationToken cancellationToken = default)
        {
            var current = Identity;
            if (_verified || null == _store || null == _identityProvider || null == current.Ticket) return;

            var ticket = _store.ValidateTicket(current.Ticket)
                ?? throw new NotAuthenticatedException("The session has expired: log in again.");
            var found = await _store.FindAsync(ticket, _identityProvider, cancellationToken).ConfigureAwait(false)
                ?? throw new NotAuthenticatedException("The session has ended: log in again.");

            if (found.Ticket != current.Ticket || found.Stamp != current.Stamp)
                await NotifyIdentityChangedAsync(found).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual Task NotifyIdentityChangedAsync(CarmaIdentity refreshed)
        {
            if (null == refreshed) throw new ArgumentNullException(nameof(refreshed));

            CarmaIdentity old, current;
            lock (_lock)
            {
                old = _identity;
                if (old.IsAnonymous || refreshed.IsAnonymous || old.UserId != refreshed.UserId
                    || old.TenantId != refreshed.TenantId || old.IsSecondFactorPending != refreshed.IsSecondFactorPending)
                    throw new InvalidOperationException(
                        "The refreshed identity belongs to another session: start a new scope for it.");

                // the endpoint is the client's: the server never knows it
                current = Equals(refreshed.Endpoint, old.Endpoint) ? refreshed : refreshed.WithEndpoint(old.Endpoint);
                _identity = current;
            }

            // a new ticket alone changes nothing anyone sees
            if (old.Stamp != current.Stamp) Raise(new IdentityChangedEventArgs(old, current));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public IDisposable Subscribe(Func<IdentityChangedEventArgs, Task> handler)
        {
            if (null == handler) throw new ArgumentNullException(nameof(handler));
            lock (_lock) _handlers.Add(handler);
            return new Subscription(this, handler);
        }

        /// <summary>Drops every subscription when the scope ends.</summary>
        public void Dispose()
        {
            lock (_lock) _handlers.Clear();
        }


        /// <summary>
        /// Runs the handlers one after another, in the background: the data portal call that brought the change does
        /// not wait for them (a handler needing a UI thread that waits for the call would deadlock).
        /// </summary>
        private void Raise(IdentityChangedEventArgs args)
        {
            Func<IdentityChangedEventArgs, Task>[] handlers;
            lock (_lock) handlers = _handlers.ToArray();
            if (handlers.Length > 0) _ = RaiseAsync(handlers, args);
        }

        private async Task RaiseAsync(Func<IdentityChangedEventArgs, Task>[] handlers, IdentityChangedEventArgs args)
        {
            foreach (var handler in handlers)
            {
                try
                {
                    if (null == _dispatcher) await handler(args).ConfigureAwait(false);
                    else await _dispatcher.InvokeAsync(() => handler(args)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex);
                }
            }
        }

        private sealed class Subscription : IDisposable
        {
            private DefaultAuthenticationStateProvider _owner;
            private readonly Func<IdentityChangedEventArgs, Task> _handler;

            public Subscription(DefaultAuthenticationStateProvider owner, Func<IdentityChangedEventArgs, Task> handler)
            {
                _owner = owner;
                _handler = handler;
            }

            public void Dispose()
            {
                var owner = Interlocked.Exchange(ref _owner, null);
                if (null != owner) lock (owner._lock) owner._handlers.Remove(_handler);
            }
        }
    }
}

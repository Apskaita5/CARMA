using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Sessions
{
    /// <summary>
    /// The framework's identity store: sessions and signing keys in memory. For a single server; after a restart every
    /// ticket is refused and every user logs in again. An app with several servers, or that must keep sessions across
    /// restarts, implements <see cref="IIdentityStore"/> itself.
    /// </summary>
    [DefaultServiceImplementation(typeof(IIdentityStore), BuildConfiguration.Server)]
    public class DefaultIdentityStore : IIdentityStore
    {
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

        private readonly ConcurrentDictionary<string, Session> _sessions
            = new ConcurrentDictionary<string, Session>(StringComparer.Ordinal);
        private readonly ISecurityConfiguration _configuration;
        private readonly TicketSigner _signer;
        private long _lastSweep;


        /// <summary>Creates the store.</summary>
        public DefaultIdentityStore(ISecurityConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _signer = new TicketSigner(configuration.KeyRotationInterval, configuration.MaxSessionLifetime);
        }


        /// <summary>Gets the current time; for tests.</summary>
        protected virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;


        /// <inheritdoc />
        public TicketInfo ValidateTicket(string ticket) => _signer.Validate(ticket, UtcNow);

        /// <inheritdoc />
        public async Task<CarmaIdentity> FindAsync(TicketInfo ticket, IIdentityProvider identityProvider,
            CancellationToken cancellationToken)
        {
            if (null == ticket) throw new ArgumentNullException(nameof(ticket));
            if (null == identityProvider) throw new ArgumentNullException(nameof(identityProvider));

            if (!_sessions.TryGetValue(ticket.TicketId, out var session) || session.UserId != ticket.UserId) return null;

            await session.Lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var now = UtcNow;
                if (now >= session.ExpiresAt)
                {
                    Remove(session);
                    return null;
                }

                if (session.Version != session.CorrectedVersion)
                {
                    // invalidated: fetched again; an invalidation during the fetch leaves it invalidated
                    var version = session.Version;
                    var fetched = await identityProvider.FetchAsync(session.UserId, session.TenantId, cancellationToken)
                        .ConfigureAwait(false);

                    if (null == fetched)
                    {
                        Remove(session);
                        return null;
                    }

                    if (fetched.UserId != session.UserId) throw new InvalidOperationException(
                        $"{nameof(IIdentityProvider)} has fetched another user than {session.UserId}.");

                    session.CorrectedVersion = version;
                    Renew(session, fetched, now);
                }
                else if (_configuration.SlidingExpiration && !session.SecondFactorPending)
                {
                    Renew(session, session.Identity, now);
                }

                return session.Identity;
            }
            finally
            {
                session.Lock.Release();
            }
        }

        /// <inheritdoc />
        public Task<CarmaIdentity> StartSessionAsync(CarmaIdentity identity, string tenantId,
            IReadOnlyCollection<string> tenantPlugins, bool secondFactorPending, CancellationToken cancellationToken)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));
            if (identity.IsAnonymous) throw new ArgumentException("An anonymous identity has no session.", nameof(identity));

            var now = UtcNow;
            Sweep(now);

            var ticketId = NewTicketId();
            var lifetime = secondFactorPending ? _configuration.SecondFactorLifetime : _configuration.SessionLifetime;
            var session = new Session(ticketId, identity.UserId, string.IsNullOrWhiteSpace(tenantId) ? null : tenantId,
                tenantPlugins ?? Array.Empty<string>(), secondFactorPending, now, now + lifetime);

            Renew(session, identity, now, extend: false);
            _sessions[ticketId] = session;

            return Task.FromResult(session.Identity);
        }

        /// <inheritdoc />
        public Task EndSessionAsync(string ticketId, CancellationToken cancellationToken)
        {
            if (null != ticketId) _sessions.TryRemove(ticketId, out _);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task EndUserSessionsAsync(string userId, CancellationToken cancellationToken)
        {
            foreach (var session in _sessions.Values.Where(s => s.UserId == userId).ToList()) Remove(session);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task InvalidateUserAsync(string userId, CancellationToken cancellationToken)
        {
            foreach (var session in _sessions.Values.Where(s => s.UserId == userId)) Interlocked.Increment(ref session.Version);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task InvalidateAllAsync(CancellationToken cancellationToken)
        {
            foreach (var session in _sessions.Values) Interlocked.Increment(ref session.Version);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Gives the session its identity with a new ticket: the identity's stamp, and a later expiry if it slides.
        /// </summary>
        private void Renew(Session session, CarmaIdentity identity, DateTimeOffset now, bool extend = true)
        {
            if (extend && _configuration.SlidingExpiration && !session.SecondFactorPending)
            {
                var max = session.StartedAt + _configuration.MaxSessionLifetime;
                var next = now + _configuration.SessionLifetime;
                session.ExpiresAt = next < max ? next : max;
            }

            var ticket = _signer.Issue(new TicketInfo(session.TicketId, session.UserId, session.TenantId,
                identity.Stamp, now, session.ExpiresAt), now);

            session.Identity = identity.WithSession(ticket, session.TenantId, session.TenantPlugins,
                session.SecondFactorPending);
        }

        private void Remove(Session session)
            => ((ICollection<KeyValuePair<string, Session>>)_sessions)
                .Remove(new KeyValuePair<string, Session>(session.TicketId, session));

        private void Sweep(DateTimeOffset now)
        {
            var last = Interlocked.Read(ref _lastSweep);
            if (now.UtcTicks - last < SweepInterval.Ticks
                || Interlocked.CompareExchange(ref _lastSweep, now.UtcTicks, last) != last) return;

            foreach (var session in _sessions.Values.Where(s => now >= s.ExpiresAt).ToList()) Remove(session);
        }

        private static string NewTicketId()
        {
            var bytes = new byte[16];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
            return TicketSigner.Base64Url(bytes);
        }


        private sealed class Session
        {
            public readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);
            public int Version;
            public int CorrectedVersion;

            public Session(string ticketId, string userId, string tenantId, IReadOnlyCollection<string> tenantPlugins,
                bool secondFactorPending, DateTimeOffset startedAt, DateTimeOffset expiresAt)
            {
                TicketId = ticketId;
                UserId = userId;
                TenantId = tenantId;
                TenantPlugins = tenantPlugins;
                SecondFactorPending = secondFactorPending;
                StartedAt = startedAt;
                ExpiresAt = expiresAt;
            }

            public string TicketId { get; }
            public string UserId { get; }
            public string TenantId { get; }
            public IReadOnlyCollection<string> TenantPlugins { get; }
            public bool SecondFactorPending { get; }
            public DateTimeOffset StartedAt { get; }
            public DateTimeOffset ExpiresAt { get; set; }
            public CarmaIdentity Identity { get; set; }
        }
    }
}

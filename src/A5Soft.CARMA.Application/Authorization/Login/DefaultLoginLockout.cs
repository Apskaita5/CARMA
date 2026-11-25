using A5Soft.CARMA.Application.Authorization.Sessions;
using A5Soft.CARMA.Domain;
using System;
using System.Collections.Concurrent;

namespace A5Soft.CARMA.Application.Authorization.Login
{
    /// <summary>
    /// The framework's lockout: failed attempts counted in memory (one server; lost on restart).
    /// </summary>
    [DefaultServiceImplementation(typeof(ILoginLockout), BuildConfiguration.Server)]
    public class DefaultLoginLockout : ILoginLockout
    {
        private readonly ConcurrentDictionary<string, Entry> _entries
            = new ConcurrentDictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        private readonly ISecurityConfiguration _configuration;


        /// <summary>Creates the lockout.</summary>
        public DefaultLoginLockout(ISecurityConfiguration configuration)
            => _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));


        /// <summary>Gets the current time; for tests.</summary>
        protected virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;


        /// <inheritdoc />
        public TimeSpan? GetLockout(string key)
        {
            if (null == key || !_entries.TryGetValue(key, out var entry)) return null;
            lock (entry)
            {
                var remaining = entry.LockedUntil - UtcNow;
                return remaining > TimeSpan.Zero ? remaining : (TimeSpan?)null;
            }
        }

        /// <inheritdoc />
        public int RecordFailure(string key)
        {
            if (null == key) return 0;

            var entry = _entries.GetOrAdd(key, _ => new Entry());
            lock (entry)
            {
                var now = UtcNow;
                // a lockout that has passed starts the count again
                if (entry.LockedUntil != default && entry.LockedUntil <= now)
                {
                    entry.Failures = 0;
                    entry.LockedUntil = default;
                }

                entry.Failures++;
                if (entry.Failures >= _configuration.MaxFailedLogins) entry.LockedUntil = now + _configuration.LockoutDuration;
                return entry.Failures;
            }
        }

        /// <inheritdoc />
        public void RecordSuccess(string key)
        {
            if (null != key) _entries.TryRemove(key, out _);
        }


        private sealed class Entry
        {
            public int Failures;
            public DateTimeOffset LockedUntil;
        }
    }
}

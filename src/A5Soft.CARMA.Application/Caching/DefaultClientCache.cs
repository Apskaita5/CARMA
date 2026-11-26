using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Caching
{
    /// <summary>
    /// The default <see cref="IClientCache"/>: in memory, for the scope, with a lifetime per value. Empties itself
    /// when the scope's identity changes.
    /// </summary>
    /// <remarks>Thread safe. Two callers missing the same key at once both fetch, and the later one's value stays:
    /// the values are the same data, so nothing is lost.</remarks>
    [DefaultServiceImplementation(typeof(IClientCache))]
    public class DefaultClientCache : IClientCache, IDisposable
    {
        private readonly object _lock = new object();
        private readonly Dictionary<Type, Entry> _entries = new Dictionary<Type, Entry>();
        private readonly IDisposable _subscription;


        /// <summary>
        /// Creates the cache of a scope.
        /// </summary>
        /// <param name="authentication">the scope's identity: a change empties the cache, as lookups may be
        /// filtered for the identity</param>
        public DefaultClientCache(IAuthenticationStateProvider authentication)
        {
            if (null == authentication) throw new ArgumentNullException(nameof(authentication));

            _subscription = authentication.Subscribe(OnIdentityChanged);
        }


        /// <inheritdoc />
        public virtual TimeSpan DefaultLifetime => TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets the current time; for tests.
        /// </summary>
        protected virtual DateTime UtcNow => DateTime.UtcNow;


        /// <inheritdoc />
        public bool TryGet<T>(Type key, out T value)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));

            lock (_lock)
            {
                if (_entries.TryGetValue(key, out var entry))
                {
                    if (entry.Expires > UtcNow && entry.Value is T typed)
                    {
                        value = typed;
                        return true;
                    }

                    _entries.Remove(key);
                }
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        public void Set<T>(Type key, T value, TimeSpan lifetime, Type entityType = null)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));
            if (null == value || lifetime <= TimeSpan.Zero) return;

            var now = UtcNow;
            lock (_lock)
            {
                RemoveExpired(now);
                _entries[key] = new Entry(value, now + lifetime, entityType);
            }
        }

        /// <inheritdoc />
        public void Invalidate(Type key)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));

            lock (_lock) _entries.Remove(key);
        }

        /// <inheritdoc />
        public void InvalidateEntity(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));

            lock (_lock)
            {
                foreach (var key in _entries.Where(e => e.Value.EntityType == entityType)
                    .Select(e => e.Key).ToList())
                    _entries.Remove(key);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            lock (_lock) _entries.Clear();
        }

        /// <summary>
        /// Stops listening to the identity's changes when the scope ends.
        /// </summary>
        public void Dispose() => _subscription.Dispose();


        private Task OnIdentityChanged(IdentityChangedEventArgs args)
        {
            Clear();
            return Task.CompletedTask;
        }

        private void RemoveExpired(DateTime now)
        {
            foreach (var key in _entries.Where(e => e.Value.Expires <= now)
                .Select(e => e.Key).ToList())
                _entries.Remove(key);
        }


        private sealed class Entry
        {
            public Entry(object value, DateTime expires, Type entityType)
            {
                Value = value;
                Expires = expires;
                EntityType = entityType;
            }

            public object Value { get; }
            public DateTime Expires { get; }
            public Type EntityType { get; }
        }
    }
}

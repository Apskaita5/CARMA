using System;
using System.Reflection;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Sets how long the client keeps a lookup or a query result in its cache: on a lookup type, instead of the
    /// default 10 minutes; on a query result type (or the item type of a list query), to have it cached at all.
    /// </summary>
    /// <remarks>
    /// <para>Only results fetched from a remote server are cached, and only those of queries without criteria. The
    /// cached instance is shared: never change it.</para>
    /// <para>Zero minutes turns caching off for the type, e.g. for a lookup that must always be current.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ClientCacheLifetimeAttribute : Attribute
    {
        /// <summary>
        /// Sets how long the client keeps the type's values in its cache.
        /// </summary>
        /// <param name="minutes">the retention period in minutes; 0 for no caching</param>
        /// <exception cref="ArgumentOutOfRangeException">the period is negative</exception>
        public ClientCacheLifetimeAttribute(int minutes)
        {
            if (minutes < 0) throw new ArgumentOutOfRangeException(nameof(minutes), minutes,
                "A client cache lifetime cannot be negative.");

            Lifetime = TimeSpan.FromMinutes(minutes);
        }


        /// <summary>
        /// Gets how long the client keeps the type's values; <see cref="TimeSpan.Zero"/> for not at all.
        /// </summary>
        public TimeSpan Lifetime { get; }


        /// <summary>
        /// Gets the lifetime a type sets.
        /// </summary>
        /// <param name="type">a lookup or query result type</param>
        /// <returns>the lifetime; null if the type does not set one</returns>
        public static TimeSpan? GetLifetime(Type type)
            => type?.GetCustomAttribute<ClientCacheLifetimeAttribute>(true)?.Lifetime;
    }
}

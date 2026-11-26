using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.Caching
{
    /// <summary>
    /// The client's cache of values fetched from the server, for the scope: lookups, and query results whose type
    /// carries <see cref="ClientCacheLifetimeAttribute"/>. Each value expires after its own lifetime.
    /// </summary>
    /// <remarks>
    /// <para>Scoped: a scope's identity is fixed, so another user or tenant starts with an empty cache. The cache
    /// also empties itself when the scope's identity changes within the scope (e.g. its permissions).</para>
    /// <para>The values are shared by everyone who gets them: never change one. A value is kept under a type: a
    /// lookup list under its lookup type, a query result under its type (for a list query,
    /// <c>List&lt;TItem&gt;</c>), so that a use case can drop what it has changed (<see cref="Invalidate"/>).</para>
    /// <para>The use case bases use it in the remote branch only; a use case or service may use it for anything
    /// else worth keeping on the client for a while.</para>
    /// </remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IClientCache
    {
        /// <summary>
        /// Gets the lifetime of a value whose type does not set one: 10 minutes by default.
        /// </summary>
        TimeSpan DefaultLifetime { get; }

        /// <summary>
        /// Gets a value that has not expired.
        /// </summary>
        /// <typeparam name="T">the type of the value</typeparam>
        /// <param name="key">the type the value is kept under</param>
        /// <param name="value">the value; default if there is none</param>
        /// <returns>true if the cache holds a current value of the type for the key</returns>
        bool TryGet<T>(Type key, out T value);

        /// <summary>
        /// Keeps a value for a while. A value already held for the key is replaced.
        /// </summary>
        /// <typeparam name="T">the type of the value</typeparam>
        /// <param name="key">the type to keep the value under</param>
        /// <param name="value">the value; null is not kept</param>
        /// <param name="lifetime">how long to keep it; zero or less keeps nothing</param>
        /// <param name="entityType">the entity type the value depends on, if any: <see cref="InvalidateEntity"/> drops
        /// it (a lookup depends on the entity it refers to)</param>
        void Set<T>(Type key, T value, TimeSpan lifetime, Type entityType = null);

        /// <summary>
        /// Drops the value kept under a type, if any.
        /// </summary>
        /// <param name="key">the type the value is kept under, e.g. a query result type</param>
        void Invalidate(Type key);

        /// <summary>
        /// Drops every value that depends on an entity type, e.g. after an entity of the type has been created,
        /// updated or deleted.
        /// </summary>
        /// <param name="entityType">the entity type</param>
        void InvalidateEntity(Type entityType);

        /// <summary>
        /// Drops every value.
        /// </summary>
        void Clear();
    }
}

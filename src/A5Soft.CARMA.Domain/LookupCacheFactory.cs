using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Factory for creating LookupCache instances without knowing TEntity at compile time.
    /// Uses reflection to determine TEntity from TLookup's base class.
    /// </summary>
    public static class LookupCacheFactory
    {
        private static readonly ConcurrentDictionary<Type, Type> _cache = new ConcurrentDictionary<Type, Type>();


        /// <summary>
        /// Creates a LookupCache for the given lookups.
        /// Automatically determines TEntity from TLookup's LookupBase&lt;TEntity&gt; inheritance.
        /// </summary>
        /// <typeparam name="TLookup">The lookup type (must derive from LookupBase&lt;T&gt;)</typeparam>
        /// <param name="lookups">The lookup instances to cache</param>
        /// <returns>A LookupCache instance wrapped in ILookupCache interface</returns>
        /// <exception cref="InvalidOperationException">If TLookup doesn't derive from LookupBase&lt;T&gt;</exception>
        public static ILookupCache<TLookup> Create<TLookup>(IEnumerable<TLookup> lookups)
            where TLookup : class
        {
            var cacheType = GetCacheType<TLookup>();

            var cache = Activator.CreateInstance(cacheType, new object[] { lookups });

            return (ILookupCache<TLookup>)cache;
        }

        /// <summary>
        /// Gets a LookupCache type for the given lookups.
        /// Automatically determines TEntity from TLookup's <see cref="LookupBase{T}"/> inheritance.
        /// </summary>
        /// <typeparam name="TLookup">The lookup type (must derive from <see cref="LookupBase{T}"/>)</typeparam>
        /// <returns>A LookupCache instance wrapped in <see cref="ILookupCache{TLookup}"/> interface</returns>
        /// <exception cref="InvalidOperationException">If <typeparamref name="TLookup"/>
        /// doesn't derive from <see cref="LookupBase{T}"/></exception>
        public static Type GetCacheType<TLookup>()
            where TLookup : class
        {
            var lookupType = typeof(TLookup);

            if (_cache.TryGetValue(lookupType, out var cachedResult)) return cachedResult;

            // Find LookupBase<TEntity> in the inheritance chain
            var entityType = FindLookupEntityType(lookupType);
            if (null == entityType) throw new InvalidOperationException(
                $"Type {lookupType.Name} does not derive from LookupBase<T>");

            // Create LookupCache<TEntity, TLookup> using reflection
            var cacheType = typeof(LookupCache<,>).MakeGenericType(entityType, lookupType);

            // Cache the type
            _cache.TryAdd(lookupType, cacheType);

            return cacheType;
        }

        /// <summary>
        /// Finds the LookupBase referenced entity type in the inheritance chain.
        /// </summary>
        private static Type FindLookupEntityType(Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(LookupBase<>))
                {
                    return type.GetGenericArguments()[0];
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}

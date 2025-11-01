using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// High-performance cache for lookup entities - Dictionary + List approach.
    /// Optimized for GetAll() performance at cost of additional memory.
    /// Implements IEnumerable for direct iteration support.
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity being referenced</typeparam>
    /// <typeparam name="TLookup">Type of the lookup class (must derive from LookupBase)</typeparam>
    public class LookupCache<TEntity, TLookup> : IEnumerable<TLookup>
        where TLookup : LookupBase<TEntity>
    {
        private readonly Dictionary<DomainEntityIdentity<TEntity>, TLookup> _lookupById;
        private readonly IReadOnlyCollection<TLookup> _readOnlyCollection;


        /// <summary>
        /// Initializes a new instance of the LookupCache class that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="items">The collection whose elements are copied to the new LookupCache.</param>
        /// <returns>
        /// An exception if the collection of items is null, or if any of the id values associated with the items within the collection are null.
        /// </returns>
        public LookupCache(IEnumerable<TLookup> items)
        {
            if (null == items) throw new ArgumentNullException(nameof(items));
            if (items.Any(i => null == i.Id)) throw new InvalidOperationException(
                $"Lookup cannot have a null id value ({items.ToString()}).");

            _readOnlyCollection = items.ToList().AsReadOnly();

            _lookupById = new Dictionary<DomainEntityIdentity<TEntity>, TLookup>(items.Count());
            foreach (var item in items)
            {
                _lookupById[item.Id] = item;
            }
        }


        /// <summary>
        /// Returns all lookups as a <see cref="IReadOnlyCollection{TLookup}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReadOnlyCollection<TLookup> GetAll() => _readOnlyCollection;

        /// <summary>
        /// Finds an instance of <typeparamref name="TLookup"/> by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to find.</param>
        /// <returns>
        /// The instance of <typeparamref name="TLookup"/> with the specified ID,
        /// or null if no such instance exists.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLookup FindById(DomainEntityIdentity<TEntity> id)
        {
            return _lookupById.TryGetValue(id, out var result) ? result : null;
        }

        /// <summary>
        /// Checks if the collection contains a lookup with the specified id.
        /// </summary>
        /// <param name="id">The identity of the entity.</param>
        /// <returns>
        /// True if the specified lookup is found in the collection; otherwise false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(DomainEntityIdentity<TEntity> id)
        {
            return _lookupById.ContainsKey(id);
        }

        /// <summary>
        /// Attempts to fetch a lookup instance for the given Id.
        /// </summary>
        /// <param name="id">The id of the entity to find.</param>
        /// <param name="lookup">Output parameter for the retrieved lookup</param>
        /// <returns>
        /// Returns true if the lookup is successfully retrieved. Else returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetById(DomainEntityIdentity<TEntity> id, out TLookup lookup)
        {
            return _lookupById.TryGetValue(id, out lookup);
        }

        /// <summary>
        /// Gets the total number of items in the cache.
        /// </summary>
        public int Count => _readOnlyCollection.Count;

        /// <summary>
        /// Gets the enumerator for the lookup collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<TLookup> GetEnumerator() => _readOnlyCollection.GetEnumerator();

        /// <summary>
        /// Implements the GetEnumerator method of the IEnumerable interface, which exposes an enumerator that supports a simple iteration over a non-generic collection.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
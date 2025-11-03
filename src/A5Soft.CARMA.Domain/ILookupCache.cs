using System.Collections.Generic;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Cache interface for type agnostic lookup (DB ORM operations).
    /// </summary>
    public interface ILookupCache<TLookup> : IEnumerable<TLookup>
        where TLookup : class
    {
        /// <summary>
        /// Gets a lookup instance by normalized key (Trim().ToLowerInvariant()) or null if no such lookup.
        /// </summary>
        /// <param name="normalizedKey">a normalized key (Trim().ToLowerInvariant()) to find</param>
        /// <returns>a lookup instance for <paramref name="normalizedKey"/> or null if no such key</returns>
        TLookup FindByKey(string normalizedKey);
    }
}

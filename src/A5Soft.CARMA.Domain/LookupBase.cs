using System;
using System.Runtime.CompilerServices;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A base class for lookups (implements equality overloads).
    /// </summary>
    /// <remarks>The LookupBase implementation is fully compatible with HashSet and Dictionary out of the box.</remarks>
    public abstract class LookupBase<T> : ILookup<T>, IEquatable<LookupBase<T>>
    {
        /// <summary>
        /// An id (primary key) of the referenced entity.
        /// </summary>
        protected DomainEntityIdentity<T> _id;


        /// <summary>
        /// An id of the referenced entity.
        /// </summary>
        public DomainEntityIdentity<T> Id => _id;


        #region Equality Operations

        /// <summary>
        /// Overloads the equality operator for two instances of LookupBase.
        /// </summary>
        /// <param name="first">The first instance of LookupBase.</param>
        /// <param name="second">The second instance of LookupBase.</param>
        /// <returns>
        /// Returns true if both instances are the same or both are null, otherwise false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(LookupBase<T> first, LookupBase<T> second)
        {
            // Use ReferenceEquals for null checks - faster than 'is null'
            if (ReferenceEquals(first, null)) return ReferenceEquals(second, null);
            return first.Equals(second);
        }

        /// <summary>
        /// Determines whether two LookUp instances are not equal.
        /// </summary>
        /// <param name="first">The first instance of LookUp.</param>
        /// <param name="second">The second instance of LookUp.</param>
        /// <returns>
        /// Returns true if they are not equal, otherwise false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(LookupBase<T> first, LookupBase<T> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Converts the <see cref="LookupBase{T}"/> object to <see cref="DomainEntityIdentity{T}"/>.
        /// Automatically retrieves the <see cref="LookupBase{T}.Id"/> property if the <paramref name="lookup"/> is not null.
        /// </summary>
        /// <param name="lookup">The <see cref="LookupBase{T}"/> object to be converted.</param>
        public static implicit operator DomainEntityIdentity<T>(LookupBase<T> lookup)
        {
            return lookup?._id;
        }

        #endregion

        #region IDomainEntityReference Implementation

        /// <inheritdoc cref="IDomainEntityReference.DomainEntityType"/>
        Type IDomainEntityReference.DomainEntityType => typeof(T);

        /// <inheritdoc cref="IDomainEntityReference.DomainEntityType"/>
        string IDomainEntityReference.DomainEntityKey => _id?.Key ?? string.Empty;

        #endregion

        #region ISearchable Implementation

        /// <summary>
        /// Gets a value indicating whether the lookup matches the search string.
        /// </summary>
        /// <param name="searchString">a search string to compare to</param>
        /// <returns>a value indicating whether the lookup matches the search string</returns>
        public abstract bool Match(string searchString);

        #endregion

        #region Equality Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current lookup.
        /// Optimized to leverage DomainEntityIdentity's cached hash code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            // Fast path: check exact type match first
            if (obj is LookupBase<T> lookup)
                return Equals(lookup);

            return false;
        }

        /// <summary>
        /// Determines whether the specified lookup is equal to the current lookup.
        /// Type-safe equality without boxing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(LookupBase<T> other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (ReferenceEquals(_id, null)) return ReferenceEquals(other._id, null);

            // Delegate to DomainEntityIdentity's optimized equality
            // This uses cached hash code for fast rejection
            return _id == other._id;
        }

        /// <summary>
        /// Returns the hash code for this lookup.
        /// Leverages DomainEntityIdentity's pre-computed hash code for maximum performance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            // Use DomainEntityIdentity's cached hash code directly
            // No string allocation or computation needed
            return _id?.GetHashCode() ?? 0;
        }

        #endregion
    }
}

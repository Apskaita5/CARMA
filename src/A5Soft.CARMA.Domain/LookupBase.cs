using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A base class for lookups (implements equality overloads).
    /// </summary>
    public abstract class LookupBase<T> : ILookup<T>
    {
        protected DomainEntityIdentity<T> _id;


        /// <summary>
        /// An id of the referenced entity.
        /// </summary>
        public DomainEntityIdentity<T> Id
            => _id;


        public static bool operator ==(LookupBase<T> a, LookupBase<T> b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(null, b)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(null, b)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(LookupBase<T> a, LookupBase<T> b)
        {
            return !(a == b);
        }

        public static implicit operator DomainEntityIdentity<T>(LookupBase<T> lookup)
        {
            if (object.ReferenceEquals(lookup, null)) return null;
            return lookup.Id;
        }


        Type IDomainEntityReference.DomainEntityType => typeof(T);

        string IDomainEntityReference.DomainEntityKey => _id?.Key ?? string.Empty;


        /// <summary>
        /// Gets a value indicating whether the lookup matches the search string.
        /// </summary>
        /// <param name="searchString">a search string to compare to</param>
        /// <returns>a value indicating whether the lookup matches the search string</returns>
        public abstract bool Match(string searchString);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is ILookup<T> lookup)
            {
                if (null == Id && null == lookup?.Id) return true;
                if (null == Id || null == lookup?.Id) return false;

                return Id == lookup.Id;
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (null == Id) return string.Empty.GetHashCode();
            return Id.ToString().GetHashCode();
        }
    }
}

using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A base class for lookups (implements equality overloads).
    /// </summary>
    public abstract class LookupBase<T> : ILookup<T>, IDomainEntityReference
    {
        protected DomainEntityIdentity<T> _id;


        /// <summary>
        /// An id of the referenced entity.
        /// </summary>
        public DomainEntityIdentity<T> Id 
            => _id;


        public static bool operator ==(LookupBase<T> a, LookupBase<T> b)
        {
            if (a.IsNull() && b.IsNull()) return true;
            if (a.IsNull() || b.IsNull()) return false;
            return a.Equals(b);
        }
         
        public static bool operator !=(LookupBase<T> a, LookupBase<T> b)
        {
            return !(a == b);
        }

        Type IDomainEntityReference.DomainEntityType => typeof(T);

        string IDomainEntityReference.DomainEntityKey => _id?.Key ?? string.Empty;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is ILookup<T> lookup)
            {
                if (null == Id && null == lookup.Id) return true;
                if (null == Id || null == lookup.Id) return false;

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

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A base class for lookups (implements equality overloads).
    /// </summary>
    public abstract class LookupBase : ILookup
    {
        protected IDomainEntityIdentity _id;

        /// <summary>
        /// An id of the referenced entity.
        /// </summary>
        public IDomainEntityIdentity Id 
            => _id;


        public static bool operator ==(LookupBase a, LookupBase b)
        {
            if (a.IsNull() && b.IsNull()) return true;
            if (a.IsNull() || b.IsNull()) return false;
            return a.Equals(b);
        }
         
        public static bool operator !=(LookupBase a, LookupBase b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is ILookup lookup)
            {
                if (Id.IsNullOrNew() && lookup.Id.IsNullOrNew()) return true;
                if (Id.IsNullOrNew() || lookup.Id.IsNullOrNew()) return false;

                return Id.IsSameIdentityAs(lookup.Id);
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (Id.IsNullOrNew()) return string.Empty.GetHashCode();
            return Id.ToString().GetHashCode();
        }
    }
}

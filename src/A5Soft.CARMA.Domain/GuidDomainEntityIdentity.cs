using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// <see cref="IDomainEntityIdentity"/> implementation for Guid primary key.
    /// </summary>
    public class GuidDomainEntityIdentity : IDomainEntityIdentity
    {
        /// <summary>
        /// Creates an GuidDomainEntityIdentity instance for an existing entity of type <paramref name="domainEntityType"/>
        /// that has a (primary) key <paramref name="key"/>. 
        /// </summary>
        /// <param name="key">a (primary) key of the entity</param>
        /// <param name="domainEntityType">a type of the entity</param>
        public GuidDomainEntityIdentity(Guid key, Type domainEntityType)
        {
            if (!key.IsValidKey()) throw new ArgumentException(
                $"Identity key value {key} is invalid for entity type {domainEntityType?.FullName}.");

            Key = key;
            DomainEntityType = domainEntityType ?? throw new ArgumentNullException(nameof(domainEntityType));
        }

        /// <summary>
        /// Creates an IntDomainEntityIdentity instance for a new entity of type <paramref name="domainEntityType"/>. 
        /// </summary>
        /// <param name="domainEntityType">a type of the entity</param>
        public GuidDomainEntityIdentity(Type domainEntityType)
        {
            Key = null;
            DomainEntityType = domainEntityType ?? throw new ArgumentNullException(nameof(domainEntityType));
        }



        /// <summary>
        /// Gets a (typed) value of the underlying identity field (nullable for new entities).
        /// </summary>
        public Guid? Key { get; }

        /// <inheritdoc cref="IDomainEntityIdentity.DomainEntityType"/>
        public Type DomainEntityType { get; }

        /// <summary>
        /// Gets a type of the underlying identity field, i.e. Guid.
        /// </summary>
        public Type IdentityValueType => typeof(Guid);

        /// <summary>
        /// Gets a value of the underlying identity field, i.e. boxed Guid? (nullable for new entities).
        /// </summary>
        public object IdentityValue => Key;

        /// <inheritdoc cref="IDomainEntityIdentity.IsNew"/>
        public bool IsNew => !Key.HasValue;


        

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is GuidDomainEntityIdentity otherIdentity)
            {
                if (this.DomainEntityType != otherIdentity.DomainEntityType) return 1;

                if (!this.Key.HasValue && !otherIdentity.Key.HasValue) return 0;
                if (!this.Key.HasValue) return -1;
                else if (!otherIdentity.Key.HasValue) return 1;

                return Key.Value.CompareTo(otherIdentity.Key.Value);
            }
            throw new ArgumentException("Object is not GuidDomainEntityIdentity.");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!obj.IsNull() && !(obj is GuidDomainEntityIdentity)) return false;

            var typed = obj as GuidDomainEntityIdentity;
            
            if (IsNew && (typed?.IsNew ?? true)) return true;
            if (IsNew || (typed?.IsNew ?? true)) return false;

            return Key.Value == typed.Key.Value;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Key.HasValue) return $"{DomainEntityType.FullName}:{Key.Value: N}";
            return $"{DomainEntityType.FullName}:";
        }
    }
}

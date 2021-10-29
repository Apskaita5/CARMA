using System;
using System.Globalization;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// <see cref="IDomainEntityIdentity"/> implementation for int primary key.
    /// </summary>
    public class IntDomainEntityIdentity : IDomainEntityIdentity
    {
        /// <summary>
        /// Creates an IntDomainEntityIdentity instance for an existing entity of type <paramref name="domainEntityType"/>
        /// that has a (primary) key <paramref name="key"/>. 
        /// </summary>
        /// <param name="key">a (primary) key of the entity</param>
        /// <param name="domainEntityType">a type of the entity</param>
        public IntDomainEntityIdentity(int key, Type domainEntityType)
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
        public IntDomainEntityIdentity(Type domainEntityType)
        {
            Key = null;
            DomainEntityType = domainEntityType ?? throw new ArgumentNullException(nameof(domainEntityType));
        }



        /// <summary>
        /// Gets a (typed) value of the underlying identity field (nullable for new entities).
        /// </summary>
        public int? Key { get; }

        /// <inheritdoc cref="IDomainEntityIdentity.DomainEntityType"/>
        public Type DomainEntityType { get; }

        /// <summary>
        /// Gets a type of the underlying identity field, i.e. int.
        /// </summary>
        public Type IdentityValueType 
            => typeof(int);

        /// <summary>
        /// Gets a value of the underlying identity field, i.e. boxed int? (nullable for new entities).
        /// </summary>
        public object IdentityValue => Key;

        /// <inheritdoc cref="IDomainEntityIdentity.IsNew"/>
        public bool IsNew 
            => !Key.IsValidKey();

        /// <inheritdoc cref="IDomainEntityIdentity.IdentityStringValue"/>
        public string IdentityStringValue =>
            Key.HasValue ? Key.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;


        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is IntDomainEntityIdentity otherIdentity)
            {
                if (this.DomainEntityType != otherIdentity.DomainEntityType) return 1;

                if (!this.Key.HasValue && !otherIdentity.Key.HasValue) return 0;
                if (!this.Key.HasValue) return -1;
                else if (!otherIdentity.Key.HasValue) return 1;
                
                return Key.Value.CompareTo(otherIdentity.Key.Value);
            }
            throw new ArgumentException("Object is not IntDomainEntityIdentity.");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{DomainEntityType.Name}:{IdentityStringValue}";
        }
    }
}

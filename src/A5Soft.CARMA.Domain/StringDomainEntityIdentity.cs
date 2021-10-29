using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// <see cref="IDomainEntityIdentity"/> implementation for string primary key.
    /// </summary>
    public class StringDomainEntityIdentity : IDomainEntityIdentity
    {
        /// <summary>
        /// Creates a StringDomainEntityIdentity instance for an existing entity of type <paramref name="domainEntityType"/>
        /// that has a (primary) key <paramref name="key"/>. 
        /// </summary>
        /// <param name="key">a (primary) key of the entity</param>
        /// <param name="domainEntityType">a type of the entity</param>
        public StringDomainEntityIdentity(string key, Type domainEntityType)
        {
            if (!key.IsValidKey()) throw new ArgumentException(
                $"Identity key value {key} is invalid for entity type {domainEntityType?.FullName}.");

            Key = key;
            DomainEntityType = domainEntityType ?? throw new ArgumentNullException(nameof(domainEntityType));
        }

        /// <summary>
        /// Creates a StringDomainEntityIdentity instance for a new entity of type <paramref name="domainEntityType"/>. 
        /// </summary>
        /// <param name="domainEntityType">a type of the entity</param>
        public StringDomainEntityIdentity(Type domainEntityType)
        {
            Key = null;
            DomainEntityType = domainEntityType ?? throw new ArgumentNullException(nameof(domainEntityType));
        }


        /// <summary>
        /// Gets a (typed) value of the underlying identity field.
        /// </summary>
        public string Key { get; }

        /// <inheritdoc cref="IDomainEntityIdentity.DomainEntityType"/>
        public Type DomainEntityType { get; }

        /// <summary>
        /// Gets a type of the underlying identity field, i.e. int.
        /// </summary>
        public Type IdentityValueType
            => typeof(string);

        /// <summary>
        /// Gets a value of the underlying identity field, i.e. boxed string.
        /// </summary>
        public object IdentityValue => Key;

        /// <inheritdoc cref="IDomainEntityIdentity.IsNew"/>
        public bool IsNew
            => !Key.IsValidKey();

        /// <inheritdoc cref="IDomainEntityIdentity.IdentityStringValue"/>
        public string IdentityStringValue => Key.IsValidKey() ? Key : string.Empty;


        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is StringDomainEntityIdentity otherIdentity)
            {
                if (this.DomainEntityType != otherIdentity.DomainEntityType) return 1;

                if (!this.Key.IsValidKey() && !otherIdentity.Key.IsValidKey()) return 0;
                if (!this.Key.IsValidKey()) return -1;
                else if (!otherIdentity.Key.IsValidKey()) return 1;

                return Key.CompareTo(otherIdentity.Key);
            }
            throw new ArgumentException("Object is not StringDomainEntityIdentity.");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{DomainEntityType.Name}:{IdentityStringValue}";
        }
    }
}

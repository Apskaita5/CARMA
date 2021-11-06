using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Represents domain entity instance's unique identity value within the domain entity graph.
    /// </summary>
    /// <typeparam name="T">a type of the domain entity that is identified</typeparam>
    [Serializable]
    public class DomainEntityIdentity<T> : IDomainEntityReference
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DomainEntityIdentity{T}"/>.
        /// </summary>
        /// <param name="key">a (primary) key that identifies the domain entity instance</param>
        public DomainEntityIdentity(string key)
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(key));

            Key = key.Trim();
        }


        /// <summary>
        /// A (primary) key that identifies the domain entity instance.
        /// </summary>
        public string Key { get; }
        

        public static bool operator ==(DomainEntityIdentity<T> first, DomainEntityIdentity<T> second)
        {
            if (object.ReferenceEquals(first, null) && object.ReferenceEquals(second, null)) return true;
            if (object.ReferenceEquals(first, null) || object.ReferenceEquals(second, null)) return false;

            return first.Equals(second);
        }

        public static bool operator !=(DomainEntityIdentity<T> first, DomainEntityIdentity<T> second)
        {
            return !(first == second);
        }

        public static implicit operator string(DomainEntityIdentity<T> value) => value.Key;

        public static implicit operator DomainEntityIdentity<T>(string value)
        {
            if (value.IsNullOrWhiteSpace()) return null;
            return new DomainEntityIdentity<T>(value);
        }

        Type IDomainEntityReference.DomainEntityType => typeof(T);

        string IDomainEntityReference.DomainEntityKey => Key;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null) || 
                !typeof(DomainEntityIdentity<T>).IsAssignableFrom(obj.GetType())) return false;

            return ((DomainEntityIdentity<T>) obj).Key.Equals(Key, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return $"{typeof(T).FullName}:{Key}".GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{typeof(T).FullName}:{Key}";
        }
    }
}

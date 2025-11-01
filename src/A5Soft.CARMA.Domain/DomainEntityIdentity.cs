using System;
using System.Runtime.CompilerServices;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Represents domain entity instance's unique identity value within the domain entity graph.
    /// </summary>
    /// <typeparam name="T">a type of the domain entity that is identified</typeparam>
    /// <remarks>The class is fully compartable with a HashSet and a Dictionary out of the box.</remarks>
    [Serializable]
    public sealed class DomainEntityIdentity<T> : IEquatable<DomainEntityIdentity<T>>, IDomainEntityReference
    {
        private readonly int _hashCode;


        /// <summary>
        /// Creates a new instance of the <see cref="DomainEntityIdentity{T}"/>.
        /// </summary>
        /// <param name="key">a (primary) key that identifies the domain entity instance</param>
        public DomainEntityIdentity(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            Key = key.Trim().ToLowerInvariant();
            _hashCode = Key.GetHashCode(StringComparison.Ordinal);
        }


        /// <summary>
        /// A (primary) key that identifies the domain entity instance.
        /// </summary>
        public string Key { get; }


        /// <summary>
        /// Overloads the equality operator for the DomainEntityIdentity class.
        /// </summary>
        /// <param name="first">The first DomainEntityIdentity object to compare.</param>
        /// <param name="second">The second DomainEntityIdentity object to compare.</param>
        /// <returns>
        /// True if both objects are null, or if the first object equals the second one. False otherwise.
        /// </returns>
        public static bool operator ==(DomainEntityIdentity<T> first, DomainEntityIdentity<T> second)
        {
            if (ReferenceEquals(first, null)) return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        /// <summary>
        /// Overrides the not equal (!=) operator for the DomainEntityIdentity objects.
        /// </summary>
        /// <param name="first">The first DomainEntityIdentity object.</param>
        /// <param name="second">The second DomainEntityIdentity object.</param>
        /// <returns>
        /// Returns a boolean indicating whether the two provided DomainEntityIdentity objects are not equal.
        /// </returns>
        public static bool operator !=(DomainEntityIdentity<T> first, DomainEntityIdentity<T> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Defines an implicit conversion of a DomainEntityIdentity to a string by returning its key.
        /// </summary>
        public static implicit operator string(DomainEntityIdentity<T> value) => value?.Key;

        /// <summary>
        /// Defines an implicit operator for the DomainEntityIdentity. Transforms a string value into a DomainEntityIdentity of a given type.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>
        /// A new instance of the DomainEntityIdentity for a specific type if the string value isn't null or white space.
        /// Returns null if the string value is null or white space.
        /// </returns>
        public static implicit operator DomainEntityIdentity<T>(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return new DomainEntityIdentity<T>(value);
        }


        /// <inheritdoc cref="IDomainEntityReference.DomainEntityType"/>
        Type IDomainEntityReference.DomainEntityType => typeof(T);

        /// <inheritdoc cref="IDomainEntityReference.DomainEntityKey"/>
        string IDomainEntityReference.DomainEntityKey => Key;


        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is DomainEntityIdentity<T> other && this.Equals(other);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DomainEntityIdentity<T> other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Fast rejection via hash
            if (_hashCode != other._hashCode) return false;

            // Direct string equality (already normalized)
            return Key == other.Key;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _hashCode;

        /// <inheritdoc />
        public override string ToString() => Key;
    }
}

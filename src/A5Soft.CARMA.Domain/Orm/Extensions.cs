using System;
using System.Globalization;

namespace A5Soft.CARMA.Domain.Orm
{
    public static class Extensions
    {
        public static DomainEntityIdentity<T> ToIdentity<T>(this int? key)
        {
            if (key.HasValue && key.Value > 0) return
                new DomainEntityIdentity<T>(key.Value.ToString(CultureInfo.InvariantCulture));
            return null;
        }

        public static DomainEntityIdentity<T> ToIdentity<T>(this long? key)
        {
            if (key.HasValue && key.Value > 0) return
                new DomainEntityIdentity<T>(key.Value.ToString(CultureInfo.InvariantCulture));
            return null;
        }

        public static DomainEntityIdentity<T> ToIdentity<T>(this Guid? key)
        {
            if (key.HasValue && key.Value != Guid.Empty) return
                new DomainEntityIdentity<T>(key.Value.ToString("N"));
            return null;
        }

        public static DomainEntityIdentity<T> ToIdentity<T>(this Guid key)
        {
            if (key != Guid.Empty) return
                new DomainEntityIdentity<T>(key.ToString("N"));
            return null;
        }

        public static DomainEntityIdentity<T> ToIdentity<T>(this string key)
        {
            if (!key.IsNullOrWhiteSpace()) return new DomainEntityIdentity<T>(key);
            return null;
        }

        public static int AsInteger<T>(this DomainEntityIdentity<T> identity)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));
            if (!int.TryParse(identity.Key, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                throw new ArgumentException($"Key value {identity.Key} is not an integer.", nameof(identity));
            if (result < 1)
                throw new ArgumentException($"Integer key value {result} is out of range.", nameof(identity));
            return result;
        }

        public static int? AsNullableInteger<T>(this DomainEntityIdentity<T> identity)
        {
            if (null == identity || identity.Key.IsNullOrWhiteSpace()) return null;

            if (!int.TryParse(identity.Key, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                throw new ArgumentException($"Key value {identity.Key} is not an integer.", nameof(identity));

            if (result < 1) return null;

            return result;
        }

        public static long AsLong<T>(this DomainEntityIdentity<T> identity)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));
            if (!long.TryParse(identity.Key, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                throw new ArgumentException($"Key value {identity.Key} is not a int64.", nameof(identity));
            if (result < 1)
                throw new ArgumentException($"int64 key value {result} is out of range.", nameof(identity));
            return result;
        }

        public static long? AsNullableLong<T>(this DomainEntityIdentity<T> identity)
        {
            if (null == identity || identity.Key.IsNullOrWhiteSpace()) return null;

            if (!long.TryParse(identity.Key, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                throw new ArgumentException($"Key value {identity.Key} is not a int64.", nameof(identity));

            if (result < 1) return null;

            return result;
        }

        public static Guid AsGuid<T>(this DomainEntityIdentity<T> identity)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));
            if (!Guid.TryParse(identity.Key, out var result) || result == Guid.Empty)
                throw new ArgumentException($"Key value {identity.Key} is not a Guid.", nameof(identity));
            return result;
        }

        public static Guid? AsNullableGuid<T>(this DomainEntityIdentity<T> identity)
        {
            if (null == identity || identity.Key.IsNullOrWhiteSpace()) return null;

            if (!Guid.TryParse(identity.Key, out var result))
                throw new ArgumentException($"Key value {identity.Key} is not a Guid.", nameof(identity));

            if (result == Guid.Empty) return null;

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this string value)
        {
            return !value.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this int value)
        {
            return value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this int? value)
        {
            return value.HasValue && value.Value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this long value)
        {
            return value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this long? value)
        {
            return value.HasValue && value.Value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this Guid value)
        {
            return value != Guid.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty;
        }
    }
}

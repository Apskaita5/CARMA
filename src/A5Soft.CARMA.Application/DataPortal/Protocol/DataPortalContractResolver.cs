using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// A contract resolver for the values that cross the data portal (and the repository
    /// boundary): DTOs and POCOs as they are, read-only types (query results, lookups) through
    /// their constructor marked with <see cref="SerializationConstructorAttribute"/>.
    /// </summary>
    /// <remarks>
    /// <para>A serialization constructor may be private. Each of its parameters is matched,
    /// ignoring case, to a serialized property of the same name and type; a parameter without
    /// one is reported when the type is first used, rather than silently left at its default.</para>
    /// <para>A <see cref="QueryResultBase"/> has <see cref="QueryResultBase.CalculateSubtotals"/>
    /// invoked once it has been deserialized, so its derived columns are always consistent with
    /// the data and the serialization constructor need not take them.</para>
    /// </remarks>
    public class DataPortalContractResolver : DefaultContractResolver
    {
        private static readonly HashSet<Type> SafeKeyTypes = new HashSet<Type>
        {
            typeof(string), typeof(Guid), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(decimal)
        };
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> Constructors
            = new ConcurrentDictionary<Type, ConstructorInfo>();


        /// <summary>
        /// Gets the constructor of the type marked with <see cref="SerializationConstructorAttribute"/>,
        /// or null if it has none.
        /// </summary>
        /// <param name="type">a type to inspect</param>
        /// <exception cref="InvalidOperationException">more than one constructor is marked</exception>
        public static ConstructorInfo GetSerializationConstructor(Type type)
        {
            if (null == type) throw new ArgumentNullException(nameof(type));

            return Constructors.GetOrAdd(type, t =>
            {
                var marked = t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(c => c.IsDefined(typeof(SerializationConstructorAttribute), false))
                    .ToList();

                if (marked.Count > 1) throw new InvalidOperationException(
                    $"Type {t} has more than one constructor marked with [SerializationConstructor].");

                var constructor = marked.FirstOrDefault();

                // by convention a serialization constructor is private: it exists for the serializer only,
                // and nothing else may build a read-only object (a query result, a lookup) from raw values
                if (null != constructor && !constructor.IsPrivate) throw new InvalidOperationException(
                    $"The serialization constructor of {t} must be private.");

                return constructor;
            });
        }


        /// <summary>
        /// Refuses a dictionary whose keys could name or build an arbitrary type.
        /// </summary>
        /// <remarks>Json.NET never passes dictionary keys through converters: a <see cref="Type"/> key is resolved
        /// with <see cref="Type.GetType(string)"/> from whatever name the JSON holds, and a key of another type is
        /// built by its type converter from the string. So only strings, enums and primitive-like values can be
        /// keys, the same rule as <see cref="DataPortalConventions"/>, enforced here because the app's own
        /// assemblies are not validated at startup. The contract is refused before a single key is read.</remarks>
        /// <exception cref="JsonSerializationException">the key type is not allowed</exception>
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);

            var keyType = contract.DictionaryKeyType;
            if (null != keyType && !IsSafeKeyType(keyType)) throw new JsonSerializationException(
                $"Dictionary type {objectType} has key type {keyType}; only strings, enums and primitive-like " +
                "values can be dictionary keys in the data portal.");

            return contract;
        }

        /// <inheritdoc />
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            var constructor = GetSerializationConstructor(objectType);
            if (null != constructor)
            {
                var parameters = CreateConstructorParameters(constructor, contract.Properties);

                // Json.NET feeds a parameter only from a property of the same name and type;
                // any other parameter would silently get its default value
                var unmatched = constructor.GetParameters()
                    .Where(p => !contract.Properties.Any(c => !c.Ignored
                        && string.Equals(c.PropertyName, p.Name, StringComparison.OrdinalIgnoreCase)
                        && c.PropertyType == p.ParameterType))
                    .Select(p => p.Name)
                    .ToList();

                if (unmatched.Count > 0) throw new InvalidOperationException(
                    $"The serialization constructor of {objectType} has parameter(s) " +
                    $"{string.Join(", ", unmatched)} with no serialized property of the same name and type.");

                contract.OverrideCreator = args => constructor.Invoke(args);
                contract.CreatorParameters.Clear();
                foreach (var parameter in parameters) contract.CreatorParameters.Add(parameter);
            }

            if (typeof(QueryResultBase).IsAssignableFrom(objectType))
            {
                contract.OnDeserializedCallbacks.Add((value, context) =>
                    ((QueryResultBase)value).CalculateSubtotals());
            }

            return contract;
        }

        /// <summary>
        /// Gets a value indicating whether a type can be a dictionary key: a string, an enum, a primitive, or one of
        /// <see cref="Guid"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/>,
        /// <see cref="decimal"/>; nullable versions included.
        /// </summary>
        internal static bool IsSafeKeyType(Type keyType)
        {
            keyType = Nullable.GetUnderlyingType(keyType) ?? keyType;
            return keyType.IsPrimitive || keyType.IsEnum || SafeKeyTypes.Contains(keyType);
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace A5Soft.CARMA.Application.DataPortal
{
    internal static class Extensions
    {
        private static readonly JsonSerializerSettings DataPortalSerializationSettings 
            = new JsonSerializerSettings()
            {
                ContractResolver = new CustomResolver()
            };


        /// <summary>
        /// Deserializes value from json string. Handles Stream properties.
        /// If serialized value type is an interface, value is deserialized by proxy.
        /// </summary>
        /// <typeparam name="T">a type of the value to deserialize</typeparam>
        /// <param name="serializedValue">a json string that contains the serialized value</param>
        /// <returns>deserialized value of the type specified</returns>
        internal static T Deserialize<T>(this string serializedValue)
        {
            return (T)Deserialize(typeof(T), serializedValue);
        }

        /// <summary>
        /// Deserializes value from json string. Handles Stream properties.
        /// If serialized value type is an interface, value is deserialized by proxy.
        /// </summary>
        /// <param name="serializedValueType">a type of the value to deserialize</param>
        /// <param name="serializedValue">a json string that contains the serialized value</param>
        /// <returns>deserialized value of the type specified</returns>
        internal static object Deserialize(this Type serializedValueType, string serializedValue)
        {
            if (null == serializedValueType) throw new ArgumentNullException(nameof(serializedValueType));

            if (null == serializedValue) return null;

            if (serializedValueType.IsInterface)
            {
                var args = JObject.Parse(serializedValue);
                return args.ToProxy(serializedValueType);
            }

            return JsonConvert.DeserializeObject(serializedValue, serializedValueType, DataPortalSerializationSettings);
        }

        /// <summary>
        /// Serializes the value to a json string. Handles Stream properties.
        /// </summary>
        /// <typeparam name="T">a type of the value to serialize</typeparam>
        /// <param name="value">a value to serialize</param>
        /// <returns>json string containing data for the serialized value</returns>
        internal static string Serialize<T>(this T value)
        {
            return JsonConvert.SerializeObject(value, DataPortalSerializationSettings);
        }

        internal static bool IsSameIdentity(this ClaimsIdentity identity, ClaimsIdentity identityToCompare)
        {
            if (null == identity && null == identityToCompare) return true;
            if (null == identity || null == identityToCompare) return false;

            if (identity.IsAuthenticated != identityToCompare.IsAuthenticated) return false;

            if (!identity.IsAuthenticated) return true;

            if (identity.AuthenticationType != identityToCompare.AuthenticationType) return false;

            var identityClaims = identity.Claims.Select(c => c.ToComparableString()).ToList();
            var identityToCompareClaims = identity.Claims.Select(c => c.ToComparableString()).ToList();

            if (identityClaims.Count != identityToCompareClaims.Count) return false;

            identityClaims.Sort();
            identityToCompareClaims.Sort();

            for (int i = 0; i < identityClaims.Count; i++)
            {
                if (identityClaims[i] != identityToCompareClaims[i]) return false;
            }

            return true;
        }


        private static string ToComparableString(this Claim claim) 
            => $"{claim.Type}:{claim.Value}:{claim.ValueType}:{claim.Issuer}:{claim.OriginalIssuer}";

        internal class CustomResolver : DefaultContractResolver
        {
            public string ConstructorAttributeName { get; set; } = "SerializationConstructorAttribute";
            public bool IgnoreAttributeConstructor { get; set; } = false;
            public bool IgnoreSinglePrivateConstructor { get; set; } = false;
            public bool IgnoreMostSpecificConstructor { get; set; } = false;

            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                JsonObjectContract contract = base.CreateObjectContract(objectType);

                // Use default contract for non-object types.
                if (objectType.IsPrimitive || objectType.IsEnum) return contract;

                if (typeof(Stream).IsAssignableFrom(objectType))
                {
                    contract.Converter = new StreamJsonConverter();
                    return contract;
                }

                // Look for constructor with attribute first, then single private, then most specific.
                var overrideConstructor =
                       (this.IgnoreAttributeConstructor ? null : GetAttributeConstructor(objectType))
                    ?? (this.IgnoreSinglePrivateConstructor ? null : GetSinglePrivateConstructor(objectType))
                    ?? (this.IgnoreMostSpecificConstructor ? null : GetMostSpecificConstructor(objectType));

                // Set override constructor if found, otherwise use default contract.
                if (overrideConstructor != null)
                {
                    SetOverrideCreator(contract, overrideConstructor);
                }

                return contract;
            }

            private void SetOverrideCreator(JsonObjectContract contract, ConstructorInfo attributeConstructor)
            {
                contract.OverrideCreator = CreateParameterizedConstructor(attributeConstructor);
                contract.CreatorParameters.Clear();
                foreach (var constructorParameter in base.CreateConstructorParameters(attributeConstructor, contract.Properties))
                {
                    contract.CreatorParameters.Add(constructorParameter);
                }
            }

            private ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
            {
                var c = method as ConstructorInfo;
                if (c != null)
                    return a => c.Invoke(a);
                return a => method.Invoke(null, a);
            }

            protected virtual ConstructorInfo GetAttributeConstructor(Type objectType)
            {
                var constructors = objectType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(c => c.GetCustomAttributes().Any(a => a.GetType().Name == this.ConstructorAttributeName)).ToList();

                if (constructors.Count == 1) return constructors[0];
                if (constructors.Count > 1)
                    throw new JsonException($"Multiple constructors with a {this.ConstructorAttributeName}.");

                return null;
            }

            protected virtual ConstructorInfo GetSinglePrivateConstructor(Type objectType)
            {
                var constructors = objectType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

                return constructors.Length == 1 ? constructors[0] : null;
            }

            protected virtual ConstructorInfo GetMostSpecificConstructor(Type objectType)
            {
                var constructors = objectType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderBy(e => e.GetParameters().Length);

                var mostSpecific = constructors.LastOrDefault();
                return mostSpecific;
            }
        }

        private class StreamJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(Stream).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var bytes = serializer.Deserialize<byte[]>(reader);
                if (null == bytes || bytes.Length < 1) return null;
                return new MemoryStream(bytes);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (null == value)
                {
                    byte[] nullArray = null;
                    serializer.Serialize(writer, nullArray);
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ((Stream)value).CopyTo(ms);
                        serializer.Serialize(writer, ms.ToArray());
                    }
                }
            }
        }
    }
}

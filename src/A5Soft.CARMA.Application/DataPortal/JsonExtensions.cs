using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Text;
using A5Soft.CARMA.Domain;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace A5Soft.CARMA.Application.DataPortal
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings DataPortalSerializationSettings 
            = new JsonSerializerSettings() { ContractResolver = new CustomResolver() };


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
        internal static string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, DataPortalSerializationSettings);
        }


        private class CustomResolver : DefaultContractResolver
        {
            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                JsonObjectContract contract = base.CreateObjectContract(objectType);
                if (typeof(Stream).IsAssignableFrom(objectType))
                {
                    contract.Converter = new StreamJsonConverter();
                }
                return contract;
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

using A5Soft.CARMA.Application.DataPortal.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// Writes a <see cref="Type"/> value as its wire name and reads it back through
    /// <see cref="RemoteUseCaseRegistry.TrustedWireTypes"/> only, e.g. the requester type of a lookup service call.
    /// </summary>
    /// <remarks>Json.NET's own handling of a <see cref="Type"/> value would read an assembly-qualified name
    /// with <see cref="Type.GetType(string)"/>: a name chosen by the caller, able to load an assembly and
    /// reach any type. This converter replaces it everywhere; without the data portal's per-call state it
    /// refuses a type value altogether.</remarks>
    internal sealed class TrustedWireTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Type).IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (null == value)
            {
                writer.WriteNull();
                return;
            }

            var types = GetTypes(serializer);
            var type = (Type)value;

            var name = RemoteUseCaseDescriptor.GetName(type);
            if (!(types.TryGetValue(name, out var known) && known == type)) throw new JsonSerializationException(
                $"Type {type} is not a trusted wire type, so it cannot cross the data portal; only lookup types " +
                "and their entity types, domain objects and use case implementations can.");

            writer.WriteValue(name);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            if (reader.TokenType != JsonToken.String)
                throw new JsonSerializationException("A type must be written as its name.");

            var name = (string)reader.Value;
            if (null == name || !GetTypes(serializer).TryGetValue(name, out var type))
                throw new JsonSerializationException("The type named is not a trusted wire type.");

            return type;
        }

        private static IReadOnlyDictionary<string, Type> GetTypes(JsonSerializer serializer)
            => (serializer.Context.Context as FileTransfer)?.Types
                ?? throw new JsonSerializationException("A type can only be serialized by the data portal.");
    }
}
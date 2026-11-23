using System;
using A5Soft.CARMA.Domain.Files;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// Detaches files from an object graph and reattaches them, wherever they occur: a parameter,
    /// a property of a DTO or a POCO, a list item. In a request a file is written as
    /// <c>{"fileRef": n}</c> and its content travels as the n-th file part; in a response a nested
    /// file is always written as null (files travel upstream only, except a top-level download).
    /// </summary>
    /// <remarks>The converter is stateless: what it does depends on the <see cref="FileTransfer"/>
    /// placed in the serializer's streaming context for the call. Without one, it refuses to handle
    /// a file at all, so a file can never be serialized by accident.</remarks>
    internal sealed class FileContentConverter : JsonConverter
    {
        private const string RefProperty = "fileRef";


        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => typeof(FileContent).IsAssignableFrom(objectType);

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (serializer.Context.Context)
            {
                case OutgoingFiles outgoing when null != value:
                    writer.WriteStartObject();
                    writer.WritePropertyName(RefProperty);
                    writer.WriteValue(outgoing.Add((FileContent)value));
                    writer.WriteEndObject();
                    return;

                case OutgoingFiles _:
                case ResponseFiles _:
                    writer.WriteNull();
                    return;

                default:
                    throw new InvalidOperationException(
                        $"A {nameof(FileContent)} can only be serialized by the data portal.");
            }
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            if (!(serializer.Context.Context is IncomingFiles incoming))
                throw new JsonSerializationException("A file reference is not valid here.");

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("A file reference must be an object.");

            var reference = JObject.Load(reader);
            if (reference.Count != 1 || !(reference[RefProperty] is JValue index) || index.Type != JTokenType.Integer)
                throw new JsonSerializationException("A file reference must hold a single integer index.");

            return incoming.Claim(index.Value<int>());
        }
    }
}

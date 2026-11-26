using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace A5Soft.CARMA.Application.Background.Internal
{
    /// <summary>
    /// The single string a work item is stored as: "j" and its JSON, or, from <see cref="CompressionThreshold"/>
    /// characters on, "z" and the base64 of its gzipped UTF-8 JSON.
    /// </summary>
    internal static class WireFormat
    {
        /// <summary>The JSON length from which a work item is compressed.</summary>
        public const int CompressionThreshold = 1024;

        private const char Plain = 'j';
        private const char Zipped = 'z';

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MaxDepth = 8,
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };


        /// <summary>Writes a work item.</summary>
        public static string Write(object value)
        {
            var json = JsonConvert.SerializeObject(value, Settings);
            if (json.Length < CompressionThreshold) return Plain + json;

            using (var output = new MemoryStream())
            {
                using (var zip = new GZipStream(output, CompressionLevel.Optimal, true))
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    zip.Write(bytes, 0, bytes.Length);
                }
                return Zipped + Convert.ToBase64String(output.GetBuffer(), 0, (int)output.Length);
            }
        }

        /// <summary>Reads a work item.</summary>
        /// <exception cref="FormatException">the string is not a work item of the type</exception>
        public static T Read<T>(string wire) where T : class
        {
            if (string.IsNullOrEmpty(wire)) throw new FormatException("A work item is empty.");

            string json;
            try
            {
                switch (wire[0])
                {
                    case Plain:
                        json = wire.Substring(1);
                        break;
                    case Zipped:
                        using (var input = new MemoryStream(Convert.FromBase64String(wire.Substring(1))))
                        using (var zip = new GZipStream(input, CompressionMode.Decompress))
                        using (var reader = new StreamReader(zip, Encoding.UTF8))
                            json = reader.ReadToEnd();
                        break;
                    default:
                        throw new FormatException("A work item starts with an unknown format mark.");
                }

                return JsonConvert.DeserializeObject<T>(json, Settings)
                    ?? throw new FormatException($"A work item holds no {typeof(T).Name}.");
            }
            catch (Exception ex) when (!(ex is FormatException))
            {
                throw new FormatException($"A work item is not a valid {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        /// <summary>Gets the size of a work item as stored: its UTF-8 bytes.</summary>
        public static int GetSize(string wire) => Encoding.UTF8.GetByteCount(wire);

        /// <summary>A new correlation id.</summary>
        public static string NewId() => Guid.NewGuid().ToString("N");
    }
}

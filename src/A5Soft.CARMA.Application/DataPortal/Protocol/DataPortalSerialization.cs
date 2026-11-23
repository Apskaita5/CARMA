using System;
using System.Collections.Concurrent;
using A5Soft.CARMA.Application.DataPortal.Internal;
using Newtonsoft.Json;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// The JSON serializer settings for everything that crosses the data portal. Not configuration: fixed
    /// except for the depth limit, so that every transport and both sides serialize the same way.
    /// </summary>
    public static class DataPortalSerialization
    {
        /// <summary>
        /// The depth limit of the data portal's own small messages, which are read before the server's
        /// limit is known or do not depend on it: server settings, identities, errors.
        /// </summary>
        public const int PortalMessageMaxDepth = 32;

        private static readonly DataPortalContractResolver ContractResolver = new DataPortalContractResolver();
        private static readonly ConcurrentDictionary<int, JsonSerializerSettings> SettingsByDepth
            = new ConcurrentDictionary<int, JsonSerializerSettings>();


        /// <summary>
        /// Gets the serializer settings for the depth limit given. The settings are shared: do not change them.
        /// </summary>
        /// <param name="maxDepth">a max nesting depth of the JSON</param>
        public static JsonSerializerSettings GetSerializerSettings(int maxDepth)
        {
            if (maxDepth < 1) throw new ArgumentOutOfRangeException(nameof(maxDepth));

            return SettingsByDepth.GetOrAdd(maxDepth, CreateSettings);
        }

        /// <summary>
        /// Creates a serializer for a single call. A serializer is cheap to create: the contract resolver
        /// caches the contracts.
        /// </summary>
        /// <param name="maxDepth">a max nesting depth of the JSON</param>
        public static JsonSerializer CreateSerializer(int maxDepth)
            => JsonSerializer.Create(GetSerializerSettings(maxDepth));


        private static JsonSerializerSettings CreateSettings(int maxDepth)
        {
            var result = new JsonSerializerSettings
            {
                // a deeply nested payload overflows the stack, which cannot be caught
                MaxDepth = maxDepth,
                // a payload must never be able to name the type it is deserialized into
                TypeNameHandling = TypeNameHandling.None,
                // $type, $ref and $id are treated as ordinary unknown properties
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.DateTime,
                FloatParseHandling = FloatParseHandling.Decimal,
                ContractResolver = ContractResolver
            };

            // files are detached into separate parts and reattached on arrival
            result.Converters.Add(new FileContentConverter());
            // type values are resolved through the trusted wire types only, never by Type.GetType
            result.Converters.Add(new TrustedWireTypeConverter());

            return result;
        }
    }
}

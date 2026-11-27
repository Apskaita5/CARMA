using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.IoC.Plugins.Delivery
{
    /// <summary>
    /// What a server sends a Blazor WebAssembly app about its plugins: for each plugin, the files the browser loads,
    /// with their hashes (<see cref="PluginPackage"/>, <see cref="PluginDownloader"/>).
    /// </summary>
    public sealed class PluginManifest
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MaxDepth = 8,
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };


        [JsonConstructor]
        private PluginManifest() { }

        internal PluginManifest(string fingerprint, IReadOnlyList<PluginManifestEntry> plugins)
        {
            Fingerprint = fingerprint;
            Plugins = plugins;
        }


        /// <summary>Gets the server's plugin fingerprint (<see cref="PluginCatalog.Fingerprint"/>).</summary>
        [JsonProperty("fingerprint")]
        public string Fingerprint { get; private set; }

        /// <summary>Gets the plugins, by identifier.</summary>
        [JsonProperty("plugins")]
        public IReadOnlyList<PluginManifestEntry> Plugins { get; private set; } = Array.Empty<PluginManifestEntry>();


        /// <summary>Gets the manifest as JSON.</summary>
        public string ToJson() => JsonConvert.SerializeObject(this, Settings);

        /// <summary>Reads a manifest from JSON.</summary>
        /// <exception cref="FormatException">the JSON is not a manifest</exception>
        public static PluginManifest FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<PluginManifest>(json ?? string.Empty, Settings)
                    ?? throw new FormatException("The plugin manifest is empty.");
            }
            catch (JsonException ex)
            {
                throw new FormatException($"The plugin manifest is not valid: {ex.Message}", ex);
            }
        }
    }
}

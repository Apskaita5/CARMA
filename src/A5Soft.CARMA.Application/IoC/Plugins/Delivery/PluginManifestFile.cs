using Newtonsoft.Json;

namespace A5Soft.CARMA.Application.IoC.Plugins.Delivery
{
    /// <summary>
    /// A file of a plugin in the <see cref="PluginManifest"/>.
    /// </summary>
    public sealed class PluginManifestFile
    {
        [JsonConstructor]
        private PluginManifestFile() { }

        internal PluginManifestFile(string name, string sha256, long size)
        {
            Name = name;
            Sha256 = sha256;
            Size = size;
        }


        /// <summary>Gets the file name.</summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>Gets the SHA-256 of the file, lower case hex: the downloader checks it, and caches may key by it.</summary>
        [JsonProperty("sha256")]
        public string Sha256 { get; private set; }

        /// <summary>Gets the size of the file in bytes.</summary>
        [JsonProperty("size")]
        public long Size { get; private set; }
    }
}

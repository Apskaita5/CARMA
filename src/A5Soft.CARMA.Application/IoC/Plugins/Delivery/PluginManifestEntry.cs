using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.IoC.Plugins.Delivery
{
    /// <summary>
    /// A plugin in the <see cref="PluginManifest"/>.
    /// </summary>
    public sealed class PluginManifestEntry
    {
        [JsonConstructor]
        private PluginManifestEntry() { }

        internal PluginManifestEntry(string id, string version, string library, string uiLibrary,
            IReadOnlyList<PluginManifestFile> files, IReadOnlyList<string> nativeLibraries)
        {
            Id = id;
            Version = version;
            Library = library;
            UiLibrary = uiLibrary;
            Files = files;
            NativeLibraries = nativeLibraries;
        }


        /// <summary>Gets the plugin identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>Gets the plugin version.</summary>
        [JsonProperty("version")]
        public string Version { get; private set; }

        /// <summary>Gets the file name of the plugin library.</summary>
        [JsonProperty("library")]
        public string Library { get; private set; }

        /// <summary>Gets the file name of the plugin's UI companion for the browser; null for none.</summary>
        [JsonProperty("ui")]
        public string UiLibrary { get; private set; }

        /// <summary>
        /// Gets the files the browser loads: the library, its UI companion and its managed dependencies (not the UI
        /// companions for other UIs).
        /// </summary>
        [JsonProperty("files")]
        public IReadOnlyList<PluginManifestFile> Files { get; private set; } = Array.Empty<PluginManifestFile>();

        /// <summary>
        /// Gets the native libraries the plugin needs on a client: a browser cannot load them, so a Blazor WebAssembly
        /// app refuses the plugin (<see cref="PluginDownloader"/>).
        /// </summary>
        [JsonProperty("native")]
        public IReadOnlyList<string> NativeLibraries { get; private set; } = Array.Empty<string>();
    }
}

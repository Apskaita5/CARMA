using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.IoC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace A5Soft.CARMA.Application.IoC.Plugins.Delivery
{
    /// <summary>
    /// The server's side of delivering its plugins to a Blazor WebAssembly app: the manifest and the files. The app
    /// creates it once at startup and maps two endpoints to it:
    /// <code>
    /// var package = new PluginPackage(ioc.Plugins, pluginCatalog.Fingerprint, browserUiKey: "Blazor",
    ///     otherUiKeys: new[] { "WinForms", "Avalonia", "Razor" });
    ///
    /// app.MapGet("/" + PluginPackage.ManifestPath, () => Results.Text(package.ManifestJson, "application/json"));
    /// app.MapGet("/" + PluginPackage.FilesPath + "/{id}/{file}", (string id, string file, HttpResponse response) =>
    /// {
    ///     var stream = package.OpenFile(id, file);
    ///     if (null == stream) return Results.NotFound();
    ///     response.Headers.CacheControl = "public, max-age=31536000, immutable";   // the request carries the hash
    ///     return Results.Stream(stream, "application/octet-stream");
    /// });
    /// </code>
    /// </summary>
    /// <remarks>
    /// <para>The browser gets, for each plugin, every managed assembly in its folder (the library, its managed
    /// dependencies and its UI companion for the browser), not the UI companions for other UIs. It skips the
    /// assemblies it already has. Native libraries cannot reach a browser: a plugin that needs them on a client is
    /// listed with them, and the browser refuses it.</para>
    /// <para>Files are read once, when the manifest is first asked for; plugins do not change while the server runs.
    /// Only files listed in the manifest are served.</para>
    /// </remarks>
    public sealed class PluginPackage
    {
        /// <summary>The path of the manifest, relative to the server's base address.</summary>
        public const string ManifestPath = "_plugins/manifest";

        /// <summary>The path of the files, relative to the server's base address: <c>{FilesPath}/{plugin id}/{file}</c>.</summary>
        public const string FilesPath = "_plugins/files";

        private readonly IReadOnlyList<PluginAssembly> _plugins;
        private readonly string _fingerprint;
        private readonly string _browserUiKey;
        private readonly HashSet<string> _otherUiKeys;
        private readonly Lazy<(PluginManifest Manifest, string Json, Dictionary<(string, string), string> Paths)> _built;


        /// <summary>
        /// Creates the package of the server's plugins.
        /// </summary>
        /// <param name="plugins">the plugins accepted (<see cref="IoCManagerBase.Plugins"/>)</param>
        /// <param name="fingerprint">the plugin fingerprint (<see cref="PluginCatalog.Fingerprint"/>)</param>
        /// <param name="browserUiKey">the UI key of the Blazor WebAssembly app, e.g. "Blazor": the plugins' UI
        /// companions for it are sent</param>
        /// <param name="otherUiKeys">the UI keys of the other UIs, e.g. "WinForms": their UI companions are not sent</param>
        public PluginPackage(IEnumerable<PluginAssembly> plugins, string fingerprint, string browserUiKey,
            IEnumerable<string> otherUiKeys = null)
        {
            _plugins = (plugins ?? throw new ArgumentNullException(nameof(plugins))).ToList();
            _fingerprint = fingerprint;
            _browserUiKey = string.IsNullOrWhiteSpace(browserUiKey) ? null : browserUiKey.Trim();
            _otherUiKeys = new HashSet<string>((otherUiKeys ?? Enumerable.Empty<string>())
                .Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim()), StringComparer.OrdinalIgnoreCase);
            _built = new Lazy<(PluginManifest, string, Dictionary<(string, string), string>)>(Build);
        }


        /// <summary>Gets the manifest.</summary>
        public PluginManifest Manifest => _built.Value.Manifest;

        /// <summary>Gets the manifest as JSON, for the manifest endpoint.</summary>
        public string ManifestJson => _built.Value.Json;


        /// <summary>
        /// Opens a file of a plugin, for the file endpoint.
        /// </summary>
        /// <param name="pluginId">the plugin identifier</param>
        /// <param name="fileName">the file name, as in the manifest</param>
        /// <returns>the file, read only; null if the manifest does not list it</returns>
        public Stream OpenFile(string pluginId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(pluginId) || string.IsNullOrWhiteSpace(fileName)) return null;
            return _built.Value.Paths.TryGetValue((pluginId.Trim().ToLowerInvariant(), fileName.Trim().ToLowerInvariant()),
                out var path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
        }


        private (PluginManifest, string, Dictionary<(string, string), string>) Build()
        {
            var entries = new List<PluginManifestEntry>();
            var paths = new Dictionary<(string, string), string>();

            foreach (var plugin in _plugins.OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase))
            {
                var libraryFile = LibraryFileOf(plugin);
                var libraryName = Path.GetFileNameWithoutExtension(libraryFile);
                var uiFile = null == _browserUiKey ? null : $"{libraryName}.{_browserUiKey}.dll";
                var files = new List<PluginManifestFile>();
                string uiLibrary = null;

                foreach (var path in Directory.EnumerateFiles(plugin.Folder, "*.dll").OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
                {
                    var name = Path.GetFileName(path);
                    if (IsOtherUiCompanion(libraryName, name) || !IsManaged(path)) continue;
                    if (string.Equals(name, uiFile, StringComparison.OrdinalIgnoreCase)) uiLibrary = name;

                    files.Add(new PluginManifestFile(name, Sha256Of(path), new FileInfo(path).Length));
                    paths[(plugin.Id.ToLowerInvariant(), name.ToLowerInvariant())] = path;
                }

                var native = ((IEnumerable<UnmanagedAssemblyDescriptor>)AssemblyScan.Of(plugin.Assembly)
                        .ExternalDependencies?.UnmanagedAssemblies ?? Enumerable.Empty<UnmanagedAssemblyDescriptor>())
                    .Where(d => null != d && d.ForBuildConfiguration.Serves(BuildConfiguration.Client))
                    .Select(d => d.Path).ToList();

                entries.Add(new PluginManifestEntry(plugin.Id, plugin.Assembly.GetName().Version?.ToString(),
                    libraryFile, uiLibrary, files, native));
            }

            var manifest = new PluginManifest(_fingerprint, entries);
            return (manifest, manifest.ToJson(), paths);
        }

        private bool IsOtherUiCompanion(string libraryName, string fileName)
        {
            var stem = Path.GetFileNameWithoutExtension(fileName);
            if (!stem.StartsWith(libraryName + ".", StringComparison.OrdinalIgnoreCase)) return false;
            var key = stem.Substring(libraryName.Length + 1);
            return _otherUiKeys.Contains(key);
        }

        private static string LibraryFileOf(PluginAssembly plugin)
        {
            var location = plugin.Assembly.Location;
            return string.IsNullOrEmpty(location) ? plugin.Assembly.GetName().Name + ".dll" : Path.GetFileName(location);
        }

        private static bool IsManaged(string path)
        {
            try
            {
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch (BadImageFormatException)
            {
                return false;   // a native library
            }
        }

        internal static string Sha256Of(byte[] content)
        {
            using (var sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(content).Select(b => b.ToString("x2")));
        }

        private static string Sha256Of(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return string.Concat(sha.ComputeHash(stream).Select(b => b.ToString("x2")));
        }
    }
}

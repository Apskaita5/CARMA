using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.IoC.Plugins.Delivery
{
    /// <summary>
    /// The Blazor WebAssembly side of delivering plugins: downloads the server's plugins (<see cref="PluginPackage"/>)
    /// and loads them, before the IoC manager is initialized. The app calls it in <c>Program.Main</c> and returns the
    /// result from its IoC manager's <see cref="IoCManagerBase.LoadPlugins"/>:
    /// <code>
    /// var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    /// var plugins = await PluginDownloader.DownloadAsync(http,
    ///     stream => AssemblyLoadContext.Default.LoadFromStream(stream));
    /// var ioc = new AppIoCManager(..., plugins);   // its LoadPlugins override returns them
    /// </code>
    /// </summary>
    /// <remarks>
    /// <para>Assemblies the app already has (by simple name) are not downloaded. Every file is checked against its
    /// hash. A plugin that needs native libraries on a client is refused: a browser cannot load them.</para>
    /// <para>The loader is the app's, as <c>AssemblyLoadContext</c> is not part of .NET Standard 2.1; in Blazor
    /// WebAssembly it is <c>AssemblyLoadContext.Default.LoadFromStream</c>, so that the plugins resolve each other and
    /// the app's assemblies.</para>
    /// <para>The browser always gets the server's current plugins, so the client and the server run the same set; if
    /// they change, the data portal's version check makes the client reload.</para>
    /// </remarks>
    public static class PluginDownloader
    {
        /// <summary>
        /// Downloads and loads the server's plugins.
        /// </summary>
        /// <param name="http">a client whose base address is the server's (the paths of <see cref="PluginPackage"/>
        /// are relative to it)</param>
        /// <param name="load">loads an assembly from its bytes, e.g. <c>AssemblyLoadContext.Default.LoadFromStream</c></param>
        /// <param name="cancellationToken">a token to cancel the download</param>
        /// <returns>the plugins, for <see cref="IoCManagerBase.LoadPlugins"/></returns>
        /// <exception cref="NotSupportedException">a plugin needs native libraries</exception>
        /// <exception cref="InvalidOperationException">a file does not match its hash, a plugin's library is missing, or
        /// it has no <see cref="Domain.IoC.PluginIdAttribute"/></exception>
        public static async Task<IReadOnlyList<PluginAssembly>> DownloadAsync(HttpClient http, Func<Stream, Assembly> load,
            CancellationToken cancellationToken = default)
        {
            if (null == http) throw new ArgumentNullException(nameof(http));
            if (null == load) throw new ArgumentNullException(nameof(load));

            var json = await GetStringAsync(http, PluginPackage.ManifestPath, cancellationToken).ConfigureAwait(false);
            var manifest = PluginManifest.FromJson(json);

            var native = manifest.Plugins.Where(p => p.NativeLibraries.Count > 0).ToList();
            if (native.Count > 0) throw new NotSupportedException("A browser cannot load native libraries, which " +
                string.Join("; ", native.Select(p => $"plugin {p.Id} needs ({string.Join(", ", p.NativeLibraries)})")) + ".");

            var result = new List<PluginAssembly>();
            foreach (var plugin in manifest.Plugins)
            {
                Assembly library = null, ui = null;

                foreach (var file in plugin.Files)
                {
                    var name = Path.GetFileNameWithoutExtension(file.Name);
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));

                    if (null == assembly)
                    {
                        var content = await GetBytesAsync(http, $"{PluginPackage.FilesPath}/{Uri.EscapeDataString(plugin.Id)}/" +
                            $"{Uri.EscapeDataString(file.Name)}?h={file.Sha256}", cancellationToken).ConfigureAwait(false);
                        if (!string.Equals(PluginPackage.Sha256Of(content), file.Sha256, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException(
                                $"File {file.Name} of plugin {plugin.Id} does not match the server's manifest.");

                        using (var stream = new MemoryStream(content, false))
                            assembly = load(stream) ?? throw new InvalidOperationException(
                                $"The loader returned no assembly for {file.Name} of plugin {plugin.Id}.");
                    }

                    if (string.Equals(file.Name, plugin.Library, StringComparison.OrdinalIgnoreCase)) library = assembly;
                    else if (string.Equals(file.Name, plugin.UiLibrary, StringComparison.OrdinalIgnoreCase)) ui = assembly;
                }

                if (null == library) throw new InvalidOperationException(
                    $"The server's manifest does not list the library of plugin {plugin.Id}.");

                result.Add(new PluginAssembly(library, "plugins/" + plugin.Id, ui));
            }

            return result;
        }


        private static async Task<string> GetStringAsync(HttpClient http, string path, CancellationToken ct)
        {
            using (var response = await http.GetAsync(path, ct).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private static async Task<byte[]> GetBytesAsync(HttpClient http, string path, CancellationToken ct)
        {
            using (var response = await http.GetAsync(path, ct).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
        }
    }
}

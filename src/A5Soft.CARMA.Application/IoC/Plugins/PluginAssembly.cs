using A5Soft.CARMA.Application.Navigation;
using A5Soft.CARMA.Domain.IoC;
using System;
using System.IO;
using System.Reflection;

namespace A5Soft.CARMA.Application.IoC.Plugins
{
    /// <summary>
    /// A plugin loaded from the plugins folder (<see cref="IPluginsConfiguration"/>) or downloaded from the server
    /// (<see cref="Delivery.PluginDownloader"/>), with its identifier, folder and UI companion.
    /// </summary>
    public sealed class PluginAssembly
    {
        /// <summary>
        /// Creates a new plugin description.
        /// </summary>
        /// <param name="assembly">the plugin assembly, loaded by the app (<see cref="IoCManagerBase.LoadPlugin"/>)</param>
        /// <param name="folder">the plugin folder</param>
        /// <param name="uiAssembly">the plugin's UI companion for the app's UI (<see cref="IoCManagerBase.UiKey"/>),
        /// loaded by the app; null for none</param>
        /// <exception cref="InvalidOperationException">the assembly has no <see cref="PluginIdAttribute"/></exception>
        public PluginAssembly(Assembly assembly, string folder, Assembly uiAssembly = null)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentNullException(nameof(folder));
            Folder = Path.GetFullPath(folder);
            Id = assembly.GetCustomAttribute<PluginIdAttribute>()?.Id?.Trim() ?? throw new InvalidOperationException(
                $"Plugin {Folder} has no PluginId attribute: add [assembly: PluginId(\"...\")], e.g. from the " +
                "plugin projects' Directory.Build.props.");
            UiAssembly = uiAssembly;
        }


        /// <summary>Gets the plugin assembly.</summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Gets the plugin's UI companion for the app's UI (<see cref="IoCManagerBase.UiKey"/>), e.g. its Blazor
        /// components or WinForms forms; null for none. Scanned for services and forms like the plugin itself.
        /// </summary>
        public Assembly UiAssembly { get; }

        /// <summary>Gets the full path of the plugin folder; its unmanaged assembly paths are relative to it.</summary>
        public string Folder { get; }

        /// <summary>
        /// Gets the plugin identifier (<see cref="PluginIdAttribute"/>): the plugin's name in tenant assignments,
        /// identities and the plugin fingerprint.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the plugin's part of the main menu; null if it adds nothing to the menu. Set when the plugin is accepted,
        /// from its assembly scan.
        /// </summary>
        public IPluginMenu Menu { get; internal set; }


        /// <inheritdoc />
        public override string ToString() => $"{Id} {Assembly.GetName().Version} ({Folder})";
    }
}

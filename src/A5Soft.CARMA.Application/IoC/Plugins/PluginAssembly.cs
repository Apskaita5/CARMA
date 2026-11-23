using A5Soft.CARMA.Domain.IoC;
using System;
using System.IO;
using System.Reflection;

namespace A5Soft.CARMA.Application.IoC.Plugins
{
    /// <summary>
    /// A plugin loaded from the plugins folder (<see cref="IPluginsConfiguration"/>), with its identifier and folder.
    /// </summary>
    public sealed class PluginAssembly
    {
        /// <summary>
        /// Creates a new plugin description.
        /// </summary>
        /// <param name="assembly">the plugin assembly, loaded by the app (<see cref="IoCManagerBase.LoadPlugin"/>)</param>
        /// <param name="folder">the plugin folder</param>
        /// <exception cref="InvalidOperationException">the assembly has no <see cref="PluginIdAttribute"/></exception>
        internal PluginAssembly(Assembly assembly, string folder)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentNullException(nameof(folder));
            Folder = Path.GetFullPath(folder);

            Id = assembly.GetCustomAttribute<PluginIdAttribute>()?.Id?.Trim() ?? throw new InvalidOperationException(
                $"Plugin {Folder} has no PluginId attribute: add [assembly: PluginId(\"...\")], e.g. from the " +
                "plugin projects' Directory.Build.props.");
        }


        /// <summary>Gets the plugin assembly.</summary>
        public Assembly Assembly { get; }

        /// <summary>Gets the full path of the plugin folder; its unmanaged assembly paths are relative to it.</summary>
        public string Folder { get; }

        /// <summary>
        /// Gets the plugin identifier (<see cref="PluginIdAttribute"/>): the plugin's name in tenant assignments,
        /// identities and the plugin fingerprint.
        /// </summary>
        public string Id { get; }


        /// <inheritdoc />
        public override string ToString() => $"{Id} {Assembly.GetName().Version} ({Folder})";
    }
}

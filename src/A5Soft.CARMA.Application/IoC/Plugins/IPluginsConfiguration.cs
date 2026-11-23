using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.IoC.Plugins
{
    /// <summary>
    /// Where the plugins are: a folder holding one folder per plugin, each with the plugin library.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IPluginsConfiguration
    {
        /// <summary>Gets the folder holding the plugin folders; relative to the app path unless absolute.</summary>
        string Folder { get; }

        /// <summary>Gets the full path of the folder holding the plugin folders.</summary>
        string FullPath { get; }

        /// <summary>
        /// Gets or sets the plugin library file name, the same in every plugin folder (e.g. "Plugin.dll"); null for
        /// a library named after its folder ("Acme.Invoicing/Acme.Invoicing.dll").
        /// </summary>
        string LibraryName { get; }

        /// <summary>Gets the path of the plugin library in a plugin folder.</summary>
        /// <param name="pluginFolder">the full path of a plugin folder</param>
        string GetLibraryPath(string pluginFolder);
    }
}

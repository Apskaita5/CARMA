using System;
using System.IO;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// The plugins configuration, from the "Plugins" section of the app configuration.
    /// </summary>
    public sealed class PluginsConfiguration : ConfigurationBase, IPluginsConfiguration
    {
        /// <inheritdoc />
        public override Type InterfaceType => typeof(IPluginsConfiguration);

        /// <inheritdoc />
        public override string RootSection => string.Empty;

        /// <inheritdoc />
        public string Folder { get; set; } = "Plugins";

        /// <inheritdoc />
        public string LibraryName { get; set; }

        /// <inheritdoc />
        public string FullPath { get; private set; }


        /// <inheritdoc />
        public override void Init(string appPath, bool isDevelopment)
            => FullPath = Path.GetFullPath(Path.Combine(appPath, Folder ?? string.Empty));   // keeps an absolute Folder

        /// <inheritdoc />
        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(Folder)) throw new InvalidOperationException("Plugins:Folder is empty.");
            if (!string.IsNullOrWhiteSpace(LibraryName) && Path.GetFileName(LibraryName) != LibraryName)
                throw new InvalidOperationException("Plugins:LibraryName must be a file name, not a path.");
        }

        /// <inheritdoc />
        public string GetLibraryPath(string pluginFolder)
        {
            if (string.IsNullOrWhiteSpace(pluginFolder))
                throw new ArgumentNullException(nameof(pluginFolder));

            var folder = pluginFolder.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (!string.IsNullOrWhiteSpace(LibraryName))
                return Path.Combine(folder, LibraryName);

            var folderName = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentException(
                    $"Cannot derive a library name from folder path '{pluginFolder}'.", nameof(pluginFolder));

            return Path.Combine(folder, folderName + ".dll");
        }
    }
}

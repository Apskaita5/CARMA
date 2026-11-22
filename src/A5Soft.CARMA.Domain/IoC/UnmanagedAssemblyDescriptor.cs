using System;

namespace A5Soft.CARMA.Domain.IoC
{
    /// <summary>
    /// Information about an unmanaged assembly that is required for the app or plugin project (assembly).
    /// </summary>
    public sealed class UnmanagedAssemblyDescriptor
    {
        /// <summary>
        /// <para>Information about an unmanaged assembly that is required for the app or plugin project (assembly).</para>
        /// <para>Deployed unmanaged assemblies must include libs for windows (dll), linux (so) and OSX (dylib).</para>
        /// </summary>
        /// <param name="path"><para>a path to the unmanaged assembly</para>
        /// <para>Unmanaged assembly path shall not include assembly (file) extension.
        /// The extension will be added dynamically subject to the runtime environment.</para>
        /// <para>For the main app the path is relative to the app base path.</para>
        /// <para>For the plugins the path is relative to the plugin folder path.</para></param>
        public UnmanagedAssemblyDescriptor(string path, BuildConfiguration forBuildConfiguration)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            Path = path.Trim();
            ForBuildConfiguration = forBuildConfiguration;
        }

        /// <summary>
        /// Gets a path to the unmanaged assembly.
        /// </summary>
        /// <remarks><para>Each unmanaged assembly path does not include assembly (file) extension.
        /// The extension should be added dynamically subject to the runtime environment.</para>
        /// <para>For the main app the path is relative to the app base path.</para>
        /// <para>For the plugins the path is relative to the plugin folder path.</para></remarks>
        public string Path { get; }

        /// <summary>
        /// a configuration of the app build (client/server/any) that the unmanaged assembly is meant for
        /// </summary>
        public BuildConfiguration ForBuildConfiguration { get; }
    }
}

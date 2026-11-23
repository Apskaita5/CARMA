using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace A5Soft.CARMA.Application.IoC.Plugins
{
    /// <summary>
    /// The plugins installed, with a fingerprint that the client and the server compare (as they compare app
    /// versions). Built by <see cref="IoCManagerBase"/> and added to the container. Immutable.
    /// </summary>
    public sealed class PluginCatalog
    {
        private readonly HashSet<string> _ids;


        internal PluginCatalog(IEnumerable<PluginAssembly> plugins)
        {
            Plugins = plugins.OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase).ToArray();
            _ids = new HashSet<string>(Plugins.Select(p => p.Id), StringComparer.OrdinalIgnoreCase);

            // identifiers and versions only: folder names and paths may differ between the client and the server
            var text = string.Join("\n", Plugins.Select(p => $"{p.Id.ToUpperInvariant()}/{p.Assembly.GetName().Version}"));
            using (var sha = SHA256.Create())
                Fingerprint = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)), 0, 16).Replace("-", "");
        }


        /// <summary>Gets the plugins installed, by identifier.</summary>
        public IReadOnlyList<PluginAssembly> Plugins { get; }

        /// <summary>Gets a fingerprint of the plugins installed (identifiers and versions); the same for no plugins.</summary>
        public string Fingerprint { get; }


        /// <summary>Gets whether every plugin identifier given is installed.</summary>
        public bool AreInstalled(IEnumerable<string> pluginIds)
        {
            if (null == pluginIds) throw new ArgumentNullException(nameof(pluginIds));
            return pluginIds.All(_ids.Contains);
        }
    }
}

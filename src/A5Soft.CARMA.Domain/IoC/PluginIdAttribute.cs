using System;

namespace A5Soft.CARMA.Domain.IoC
{
    /// <summary>
    /// Identifies a plugin assembly: its name in tenant assignments, identities and the plugin fingerprint, the same
    /// on every machine whatever its folder is called. Required on every plugin; e.g. generated from the project
    /// name by the plugin projects' Directory.Build.props.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class PluginIdAttribute : Attribute
    {
        /// <summary>
        /// Creates a new plugin identifier.
        /// </summary>
        /// <param name="id">the plugin identifier, e.g. "Acme.Invoicing"</param>
        public PluginIdAttribute(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            Id = id.Trim();
        }

        /// <summary>Gets the plugin identifier (compared ignoring case).</summary>
        public string Id { get; }
    }
}

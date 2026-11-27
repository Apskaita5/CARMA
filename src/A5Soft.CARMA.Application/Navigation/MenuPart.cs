using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// A part of the menu: who builds it, with which resources; for a plugin, its key prefix and its assembly.
    /// </summary>
    internal sealed class MenuPart
    {
        internal MenuPart(string owner, ResourceManager resources, IMetadataProvider metadata,
            string pluginId = null, Assembly pluginAssembly = null, bool isApp = false)
        {
            Owner = owner;
            IsApp = isApp;
            Resources = resources;
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            PluginId = pluginId;
            PluginAssembly = pluginAssembly;
        }


        internal string Owner { get; }
        /// <summary>Gets whether this is the app's part (<see cref="MainMenuProviderBase.Build"/>): only the app opens
        /// its groups to plugins.</summary>
        internal bool IsApp { get; }
        internal ResourceManager Resources { get; }
        internal IMetadataProvider Metadata { get; }
        internal string PluginId { get; }
        internal Assembly PluginAssembly { get; }


        /// <summary>
        /// Gets the key of a group this part creates: a plugin's keys are prefixed with its group's key, so that they
        /// cannot clash; the app and the GUI app may not use the plugins' keys.
        /// </summary>
        internal string GroupKey(string key)
        {
            key = RequiredKey(key, "key");

            if (null != PluginId) return MainMenuProviderBase.GetPluginGroupKey(PluginId) + "." + key;

            if (IsPluginsKey(key))
                throw new ArgumentException($"The group key '{key}' is reserved for the plugins ({Owner}).", nameof(key));

            return key;
        }

        /// <summary>
        /// Gets a value indicating whether a key is the plugins' group key or the key of a group under it.
        /// </summary>
        internal static bool IsPluginsKey(string key)
            => key == MainMenuProviderBase.PluginsGroupKey
            || key.StartsWith(MainMenuProviderBase.PluginsGroupKey + ".", StringComparison.Ordinal);

        /// <summary>
        /// Gets the text of a key, read in the current UI culture on every access; checks now that it exists.
        /// </summary>
        internal Func<string> Text(string textKey)
        {
            textKey = RequiredKey(textKey, nameof(textKey));

            if (null == Resources) throw new InvalidOperationException(
                $"{Owner} has no resources for the menu text '{textKey}'.");
            if (null == Resources.GetString(textKey, CultureInfo.InvariantCulture)) throw new InvalidOperationException(
                $"The menu text '{textKey}' is missing from {Resources.BaseName} ({Owner}).");

            var resources = Resources;
            return () => resources.GetString(textKey);
        }

        /// <summary>
        /// Checks that a type is a use case interface, and the plugin's own for a plugin's part.
        /// </summary>
        internal void EnsureOwnUseCase(Type useCaseType, string parameterName)
        {
            EnsureUseCase(useCaseType, parameterName);
            if (null != PluginAssembly && useCaseType.Assembly != PluginAssembly) throw new ArgumentException(
                $"Plugin {PluginId} can only add its own use cases to the menu, not {useCaseType.FullName}.",
                parameterName);
        }

        /// <summary>
        /// Checks that a type is a use case interface, remote or local.
        /// </summary>
        internal static void EnsureUseCase(Type useCaseType, string parameterName)
        {
            if (null == useCaseType) throw new ArgumentNullException(parameterName);
            if (!useCaseType.IsInterface || !(useCaseType.IsDefined(typeof(UseCaseAttribute), false)
                || useCaseType.IsDefined(typeof(LocalUseCaseAttribute), false)))
                throw new ArgumentException($"{useCaseType.FullName} is not a use case interface (marked with " +
                    $"{nameof(UseCaseAttribute)} or {nameof(LocalUseCaseAttribute)}).", parameterName);
        }

        internal static string RequiredKey(string key, string parameterName)
            => string.IsNullOrWhiteSpace(key) ? throw new ArgumentNullException(parameterName) : key.Trim();
    }
}

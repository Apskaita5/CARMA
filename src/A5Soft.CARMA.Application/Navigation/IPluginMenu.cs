using System;
using System.Collections.Generic;
using System.Resources;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// A plugin's part of the main menu: one group in the plugins' group (wherever the app places it,
    /// <see cref="MenuBuilder.PluginsGroup"/>), and the app's use cases it replaces. At most one class per plugin
    /// assembly, with a public parameterless constructor (checked at startup). Used for the tenants the plugin is
    /// assigned to.
    /// </summary>
    /// <remarks>A plugin cannot change the app's groups (two plugins would conflict), only replace the use case an
    /// app item opens. The plugins' groups are sorted by their labels, in the current UI culture.</remarks>
    public interface IPluginMenu
    {
        /// <summary>
        /// Gets the plugin's resources for the labels of its group and subgroups.
        /// </summary>
        ResourceManager Resources { get; }

        /// <summary>
        /// Gets the key of the label of the plugin's group in <see cref="Resources"/>: the plugin's name, as the user
        /// should see it.
        /// </summary>
        string TextKey { get; }

        /// <summary>
        /// Gets the icon of the plugin's group; null (the default) for none.
        /// </summary>
        string Icon => null;

        /// <summary>
        /// Gets the app's use cases the plugin replaces in the menu: app use case interface → the plugin's own use
        /// case interface. The item keeps the app's title. Two plugins of a tenant may not replace the same one.
        /// </summary>
        /// <returns>the replacements; null (the default) for none</returns>
        IReadOnlyDictionary<Type, Type> GetUseCaseReplacements() => null;

        /// <summary>
        /// Builds the plugin's group: subgroups (their keys prefixed with the plugin's group key), the plugin's own use
        /// cases and separators.
        /// </summary>
        /// <param name="group">the builder of the plugin's group</param>
        void Build(PluginMenuBuilder group);
    }
}

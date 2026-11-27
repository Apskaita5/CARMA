using System.Resources;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// A plugin's part of the main menu: its own group in the plugins' group (wherever the app places it,
    /// <see cref="MenuBuilder.PluginsGroup"/>), and items appended to the app's groups that the app opens to plugins
    /// (<see cref="Extend"/>). At most one class per plugin assembly, with a public parameterless constructor (checked
    /// at startup). Used for the tenants the plugin is assigned to.
    /// </summary>
    /// <remarks>
    /// <para>A plugin only adds: it cannot change, reorder or remove the app's items, and it adds only its own use
    /// cases. The plugins' groups are sorted by their labels, in the current UI culture; a plugin's group that ends up
    /// without any use case item is left out of the menu.</para>
    /// <para>A plugin changes what an app's item does by other means: its own form for the app's use case, in the
    /// GUI's form registry, or its own implementation of the app's use case, in its services.</para>
    /// </remarks>
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
        /// Builds the plugin's group: subgroups (their keys prefixed with the plugin's group key), the plugin's own use
        /// cases and separators. A plugin that only extends the app's groups leaves it empty.
        /// </summary>
        /// <param name="group">the builder of the plugin's group</param>
        void Build(PluginMenuBuilder group);

        /// <summary>
        /// Appends the plugin's own items to the app's groups that are open to plugins
        /// (<see cref="MenuBuilderBase{TBuilder}.Group"/> with openToPlugins), after the app's items. The default adds
        /// nothing.
        /// </summary>
        /// <param name="app">the builder of the additions to the app's groups</param>
        void Extend(PluginMenuExtender app) { }
    }
}

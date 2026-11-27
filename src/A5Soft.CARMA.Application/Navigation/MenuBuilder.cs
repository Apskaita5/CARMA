namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// Builds the app's menu (<see cref="MainMenuProviderBase.Build"/>): groups, use case items, separators, and the
    /// place of the plugins' group (<see cref="PluginsGroup"/>). It cannot add to a group of another part: only the
    /// GUI app can (<see cref="GuiMenuBuilder.In"/>). Only the app opens a group to plugins
    /// (<see cref="MenuBuilderBase{TBuilder}.Group"/> with openToPlugins), e.g. Reports.
    /// </summary>
    public sealed class MenuBuilder : MenuBuilderBase<MenuBuilder>
    {
        internal MenuBuilder(MainMenu menu, MenuItem group, MenuPart part) : base(menu, group, part) { }


        /// <summary>
        /// Places the plugins' group here, at the top level or in any of the app's groups: a group for each of the
        /// tenant's plugins that has a menu (<see cref="IPluginMenu"/>), labelled by the plugin and sorted by the
        /// labels. Wherever it is placed, its key is <see cref="MainMenuProviderBase.PluginsGroupKey"/>.
        /// </summary>
        /// <remarks>The group is there only if a plugin of the scope's tenant has a menu; a separator left with
        /// nothing to separate is dropped. The app must place it if any plugin can have a menu.</remarks>
        /// <param name="textKey">the key of the group's label in the app's resources</param>
        /// <param name="icon">the icon of the group, if any</param>
        /// <returns>this builder</returns>
        /// <exception cref="System.InvalidOperationException">the plugins' group has been placed already</exception>
        public MenuBuilder PluginsGroup(string textKey, string icon = null)
        {
            Menu.PlacePlugins(CurrentGroup, Part, textKey, icon);

            return this;
        }


        private protected override MenuBuilder Nested(MenuItem group) => new MenuBuilder(Menu, group, Part);
    }
}

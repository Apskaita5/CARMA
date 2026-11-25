namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// Builds a plugin's group (<see cref="IPluginMenu.Build"/>): subgroups, the plugin's own use case items and
    /// separators. The keys of the subgroups are prefixed with the key of the plugin's group, so that two plugins
    /// cannot clash; nothing outside the plugin's group can be reached.
    /// </summary>
    public sealed class PluginMenuBuilder : MenuBuilderBase<PluginMenuBuilder>
    {
        internal PluginMenuBuilder(MainMenu menu, MenuItem group, MenuPart part) : base(menu, group, part) { }


        private protected override PluginMenuBuilder Nested(MenuItem group) => new PluginMenuBuilder(Menu, group, Part);
    }
}

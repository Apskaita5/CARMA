using System;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// Builds the GUI app's additions to the menu (<see cref="MainMenu.Extend"/>): besides groups, use case items and
    /// separators, actions (e.g. Exit), and additions to the app's groups (<see cref="In"/>).
    /// </summary>
    public sealed class GuiMenuBuilder : MenuBuilderBase<GuiMenuBuilder>
    {
        internal GuiMenuBuilder(MainMenu menu, MenuItem group, MenuPart part) : base(menu, group, part) { }


        /// <summary>
        /// Continues in an existing group of the app or the GUI app, wherever it is in the menu.
        /// </summary>
        /// <param name="key">the key of the group, e.g. a public constant of the app's menu provider</param>
        /// <param name="build">builds the items to add</param>
        /// <returns>this builder, to go on at this level</returns>
        /// <exception cref="ArgumentException">the key is the plugins' group's or a plugin's: those belong to the
        /// plugins</exception>
        /// <exception cref="InvalidOperationException">the menu has no group with the key</exception>
        public GuiMenuBuilder In(string key, Action<GuiMenuBuilder> build)
        {
            if (null == build) throw new ArgumentNullException(nameof(build));

            key = MenuPart.RequiredKey(key, nameof(key));
            if (MenuPart.IsPluginsKey(key)) throw new ArgumentException(
                $"The group '{key}' belongs to the plugins; {Part.Owner} cannot add to it.", nameof(key));

            var group = Menu.FindGroup(key) ?? throw new InvalidOperationException(
                $"The menu has no group with the key '{key}' ({Part.Owner}).");

            build(Nested(group));

            return this;
        }

        /// <summary>
        /// Adds an action, e.g. Settings or Exit.
        /// </summary>
        /// <param name="textKey">the key of the action's label in the GUI app's resources</param>
        /// <param name="onClick">the action</param>
        /// <param name="tag">an arbitrary object to associate with the item</param>
        /// <param name="icon">the icon of the item, if any</param>
        /// <returns>this builder</returns>
        public GuiMenuBuilder Action(string textKey, Action<MenuItem> onClick, object tag = null, string icon = null)
        {
            if (null == onClick) throw new ArgumentNullException(nameof(onClick));

            var text = Part.Text(textKey);
            Add(MenuItem.CreateAction(textKey.Trim(), text, icon, onClick, tag));

            return this;
        }


        private protected override GuiMenuBuilder Nested(MenuItem group) => new GuiMenuBuilder(Menu, group, Part);
    }
}

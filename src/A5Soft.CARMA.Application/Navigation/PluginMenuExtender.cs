using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// Builds a plugin's additions to the app's groups that are open to plugins (<see cref="IPluginMenu.Extend"/>):
    /// the plugin's own use case items, subgroups and separators, appended after the app's items.
    /// </summary>
    /// <remarks>Each plugin's additions to a group start after a separator, so that they read as a block of their own
    /// (a separator left with nothing to separate is dropped). A subgroup's key is prefixed with the plugin's group
    /// key, as in the plugin's own group.</remarks>
    public sealed class PluginMenuExtender
    {
        private readonly MainMenu _menu;
        private readonly MenuPart _part;
        private readonly HashSet<string> _extended = new HashSet<string>(StringComparer.Ordinal);


        internal PluginMenuExtender(MainMenu menu, MenuPart part)
        {
            _menu = menu;
            _part = part;
        }


        /// <summary>
        /// Appends items to a group of the app that is open to plugins, wherever it is in the menu.
        /// </summary>
        /// <param name="key">the key of the group, a public constant of the app's menu provider</param>
        /// <param name="build">builds the plugin's items</param>
        /// <returns>this extender, to extend another group</returns>
        /// <exception cref="InvalidOperationException">the menu has no group with the key, or the app has not opened it
        /// to plugins</exception>
        public PluginMenuExtender In(string key, Action<PluginMenuBuilder> build)
        {
            if (null == build) throw new ArgumentNullException(nameof(build));

            key = MenuPart.RequiredKey(key, nameof(key));
            var group = _menu.GetOpenGroup(key, _part.Owner);

            if (_extended.Add(key)) group.Add(MenuItem.CreateSeparator());
            build(new PluginMenuBuilder(_menu, group, _part));

            return this;
        }
    }
}

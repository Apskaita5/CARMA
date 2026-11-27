using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// The main menu of the scope's user, built by the <see cref="IMainMenuProvider"/>: groups and items, each use
    /// case item enabled if the user may invoke its use case.
    /// </summary>
    public sealed class MainMenu
    {
        private readonly List<MenuItem> _topItems = new List<MenuItem>();
        private readonly Dictionary<string, (MenuItem Group, string Owner, bool OpenToPlugins)> _groups
            = new Dictionary<string, (MenuItem, string, bool)>(StringComparer.Ordinal);
        private readonly IReadOnlyList<PluginAssembly> _plugins;
        private readonly List<MenuItem> _pluginGroups = new List<MenuItem>();
        private readonly IMetadataProvider _metadata;
        private readonly Action<MainMenu> _authorize;


        /// <summary>
        /// Creates an empty menu.
        /// </summary>
        /// <param name="metadata">the metadata provider, for the titles of the use cases</param>
        /// <param name="plugins">the tenant's plugins that have a menu: their groups and their additions to the
        /// app's groups</param>
        /// <param name="authorize">enables the items for the scope's current identity</param>
        internal MainMenu(IMetadataProvider metadata, IReadOnlyList<PluginAssembly> plugins, Action<MainMenu> authorize)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _plugins = plugins ?? Array.Empty<PluginAssembly>();
            _authorize = authorize ?? throw new ArgumentNullException(nameof(authorize));
        }


        /// <summary>
        /// Gets the top level items of the menu.
        /// </summary>
        public IReadOnlyList<MenuItem> TopItems => _topItems;

        /// <summary>
        /// Gets a value indicating whether the app has placed the plugins' group (<see cref="MenuBuilder.PluginsGroup"/>).
        /// </summary>
        internal bool PluginsPlaced { get; private set; }

        /// <summary>
        /// Gets a value indicating whether any of the tenant's plugins has a group of its own with a use case item
        /// (<see cref="BuildPluginGroups"/>): the app must then place the plugins' group.
        /// </summary>
        internal bool HasPluginGroups => _pluginGroups.Count > 0;


        /// <summary>
        /// Finds a group by its key, at any depth.
        /// </summary>
        /// <param name="key">the key of the group, e.g. a public constant of the app's menu provider, or
        /// <see cref="MainMenuProviderBase.PluginsGroupKey"/></param>
        /// <returns>the group; null if there is none</returns>
        public MenuItem FindGroup(string key)
            => !string.IsNullOrWhiteSpace(key) && _groups.TryGetValue(key.Trim(), out var group) ? group.Group : null;

        /// <summary>
        /// Adds the GUI app's own items: actions (e.g. Settings, Exit), use case items and groups, also to the app's
        /// groups (<see cref="GuiMenuBuilder.In"/>). Only the GUI app can change the app's groups; nobody can change
        /// the plugins' groups.
        /// </summary>
        /// <param name="resources">the GUI app's resources for the labels of its groups and actions</param>
        /// <param name="build">builds the additions</param>
        /// <returns>this menu</returns>
        public MainMenu Extend(ResourceManager resources, Action<GuiMenuBuilder> build)
        {
            if (null == build) throw new ArgumentNullException(nameof(build));

            build(new GuiMenuBuilder(this, null, new MenuPart("the GUI app", resources, _metadata)));

            TrimSeparators();
            Refresh();

            return this;
        }

        /// <summary>
        /// Brings the menu up to date: enables the items again for the scope's current identity (e.g. after its
        /// permissions have changed), and sorts the plugins' groups again by their labels in the current UI culture
        /// (e.g. after a language switch).
        /// </summary>
        public void Refresh()
        {
            SortPluginGroups();
            _authorize(this);
        }


        internal void AddTopItem(MenuItem item) => _topItems.Add(item);

        internal void RegisterGroup(MenuItem group, string owner, bool openToPlugins = false)
        {
            if (_groups.TryGetValue(group.Name, out var existing))
                throw new InvalidOperationException(
                $"The menu has a group with the key '{group.Name}' already (from {existing.Owner}); {owner} cannot " +
                "add another.");

            _groups.Add(group.Name, (group, owner, openToPlugins));
        }

        /// <summary>
        /// Builds the group of each of the tenant's plugins (<see cref="IPluginMenu.Build"/>), before the app builds
        /// its menu, and keeps those with a use case item, for <see cref="PlacePlugins"/>: a plugin that only extends
        /// the app's groups has none.
        /// </summary>
        internal void BuildPluginGroups()
        {
            foreach (var plugin in _plugins)
            {
                var part = PartOf(plugin);
                Func<string> label = null;   // checked only for a group that is kept
                var group = MenuItem.CreateGroup(MainMenuProviderBase.GetPluginGroupKey(plugin.Id),
                    () => label(), plugin.Menu.Icon);

                RegisterGroup(group, part.Owner);
                plugin.Menu.Build(new PluginMenuBuilder(this, group, part));

                if (group.Descendants().Any(i => i.ItemType == MenuItemType.UseCase))
                {
                    label = part.Text(plugin.Menu.TextKey);
                    _pluginGroups.Add(group);
                }
                else Unregister(group.Name);
            }
        }

        /// <summary>
        /// Places the plugins' group in a group (at the top level for null), with the group of each of the tenant's
        /// plugins that has one, sorted by their labels. Places nothing if there is no such plugin.
        /// </summary>
        /// <param name="parent">the group to place it in; null for the top level</param>
        /// <param name="app">the app's part of the menu</param>
        /// <param name="textKey">the key of the group's label in the app's resources</param>
        /// <param name="icon">the icon of the group, if any</param>
        /// <exception cref="InvalidOperationException">the plugins' group has been placed already</exception>
        internal void PlacePlugins(MenuItem parent, MenuPart app, string textKey, string icon)
        {
            if (PluginsPlaced) throw new InvalidOperationException(
                $"The plugins' group has been placed already; {app.Owner} can place it once only.");

            // the label is checked even without plugins, so that a missing text shows up in any test
            var text = app.Text(textKey);
            PluginsPlaced = true;

            if (_plugins.Count < 1) return;

            if (_pluginGroups.Count < 1) return;

            var pluginsGroup = MenuItem.CreateGroup(MainMenuProviderBase.PluginsGroupKey, text, icon);
            RegisterGroup(pluginsGroup, app.Owner);
            if (null == parent) AddTopItem(pluginsGroup);
            else parent.Add(pluginsGroup);

            foreach (var group in _pluginGroups) pluginsGroup.Add(group);

            SortPluginGroups();
        }

        /// <summary>
        /// Lets each of the tenant's plugins append its items to the app's groups open to plugins
        /// (<see cref="IPluginMenu.Extend"/>), after the app has built its menu: the plugins in the order of their
        /// labels in the current UI culture, each plugin's items in a group after a separator.
        /// </summary>
        /// <exception cref="InvalidOperationException">a plugin extends a group that does not exist or is not open to
        /// plugins</exception>
        internal void ApplyPluginExtensions()
        {
            var comparer = StringComparer.Create(CultureInfo.CurrentUICulture, true);
            foreach (var plugin in _plugins.OrderBy(p => LabelOf(p), comparer).ThenBy(p => p.Id, StringComparer.Ordinal))
                plugin.Menu.Extend(new PluginMenuExtender(this, PartOf(plugin)));
        }

        /// <summary>
        /// Gets a group of the app that is open to plugins.
        /// </summary>
        /// <exception cref="InvalidOperationException">there is no such group, or it is not open to plugins</exception>
        internal MenuItem GetOpenGroup(string key, string owner)
        {
            if (!_groups.TryGetValue(key, out var group)) throw new InvalidOperationException(
                $"The menu has no group with the key '{key}' for {owner} to extend.");
            if (!group.OpenToPlugins) throw new InvalidOperationException(
                $"The group '{key}' of {group.Owner} is not open to plugins; {owner} cannot extend it.");

            return group.Group;
        }

        internal IEnumerable<MenuItem> AllItems() => _topItems.SelectMany(i => new[] { i }.Concat(i.Descendants()));

        internal void TrimSeparators()
        {
            foreach (var item in _topItems) item.TrimSeparators();
        }

        internal void UpdateGroupStates()
        {
            foreach (var item in _topItems) item.UpdateGroupState();
        }


        private MenuPart PartOf(PluginAssembly plugin)
            => new MenuPart($"plugin {plugin.Id}", plugin.Menu.Resources, _metadata, plugin.Id, plugin.Assembly);

        private static string LabelOf(PluginAssembly plugin)
        {
            try
            {
                return plugin.Menu.Resources?.GetString(plugin.Menu.TextKey ?? string.Empty) ?? plugin.Id;
            }
            catch (MissingManifestResourceException)
            {
                return plugin.Id;
            }
        }

        /// <summary>Forgets a plugin's group left out of the menu, with its subgroups.</summary>
        private void Unregister(string groupKey)
        {
            foreach (var key in _groups.Keys.Where(k => k == groupKey
                || k.StartsWith(groupKey + ".", StringComparison.Ordinal)).ToList()) _groups.Remove(key);
        }

        private void SortPluginGroups()
            => FindGroup(MainMenuProviderBase.PluginsGroupKey)?.SortItems(
                StringComparer.Create(CultureInfo.CurrentUICulture, true));
    }
}

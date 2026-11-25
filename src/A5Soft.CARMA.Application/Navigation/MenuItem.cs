using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// An item of the main menu: a group (submenu), a separator, a use case or an action of the GUI app.
    /// </summary>
    /// <remarks>Built by the menu builders (<see cref="MenuBuilder"/>, <see cref="PluginMenuBuilder"/>,
    /// <see cref="GuiMenuBuilder"/>). The display name is read on every access, in the current UI culture, so a language
    /// switch needs no rebuild; <see cref="MainMenu.Refresh"/> sorts the plugins' groups again.</remarks>
    public sealed class MenuItem
    {
        private readonly Func<string> _displayName;
        private readonly List<MenuItem> _items;


        private MenuItem(MenuItemType itemType, string name, Func<string> displayName, string icon,
            Type useCaseType, Action<MenuItem> onClick, object tag)
        {
            ItemType = itemType;
            Name = name ?? string.Empty;
            _displayName = displayName ?? (() => string.Empty);
            Icon = icon ?? string.Empty;
            UseCaseType = useCaseType;
            OnClick = onClick;
            Tag = tag;
            IsEnabled = itemType != MenuItemType.Separator;
            if (itemType == MenuItemType.Submenu) _items = new List<MenuItem>();
        }


        #region Properties

        /// <summary>
        /// Gets the name of the item: for a group, its key (unique within the menu); for a use case, the full name of
        /// its interface; for an action, the key of its label.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the localized display name of the item, in the current UI culture: for a use case, its menu title
        /// (<see cref="Domain.Metadata.IUseCaseMetadata.GetMenuTitle"/>).
        /// </summary>
        public string DisplayName => _displayName();

        /// <summary>
        /// Gets the icon of the item, if any.
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        public MenuItemType ItemType { get; }

        /// <summary>
        /// Gets a value indicating whether the item can be used: a use case item if the user may invoke its use
        /// case; a group if any of its items is enabled; an action always.
        /// </summary>
        public bool IsEnabled { get; internal set; }

        /// <summary>
        /// Gets the interface of the use case a use case item opens; null for any other item.
        /// </summary>
        /// <remarks>A plugin of the tenant may have replaced the app's use case with its own: the item keeps the
        /// app's title.</remarks>
        public Type UseCaseType { get; internal set; }

        /// <summary>
        /// Gets the items of a group; empty for any other item.
        /// </summary>
        public IReadOnlyList<MenuItem> Items => (IReadOnlyList<MenuItem>)_items ?? Array.Empty<MenuItem>();

        /// <summary>
        /// Gets the group that holds the item; null for a top level item.
        /// </summary>
        public MenuItem Parent { get; private set; }

        /// <summary>
        /// Gets the action of an action item, added by the GUI app.
        /// </summary>
        public Action<MenuItem> OnClick { get; }

        /// <summary>
        /// Gets an arbitrary object the GUI app has associated with an action item.
        /// </summary>
        public object Tag { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes the action of an action item.
        /// </summary>
        /// <exception cref="InvalidOperationException">the item is not an action item</exception>
        public void InvokeOnClick()
        {
            if (null == OnClick) throw new InvalidOperationException($"Menu item {Name} has no action.");
            OnClick(this);
        }

        /// <inheritdoc />
        public override string ToString() => DisplayName;


        internal static MenuItem CreateGroup(string name, Func<string> displayName, string icon)
            => new MenuItem(MenuItemType.Submenu, name, displayName, icon, null, null, null);

        internal static MenuItem CreateUseCase(Type useCaseType, Func<string> displayName, string icon)
            => new MenuItem(MenuItemType.UseCase, useCaseType.FullName, displayName, icon, useCaseType, null, null);

        internal static MenuItem CreateAction(string name, Func<string> displayName, string icon,
            Action<MenuItem> onClick, object tag)
            => new MenuItem(MenuItemType.GuiAppItem, name, displayName, icon, null, onClick, tag);

        internal static MenuItem CreateSeparator()
            => new MenuItem(MenuItemType.Separator, null, null, null, null, null, null);

        internal void Add(MenuItem item)
        {
            if (ItemType != MenuItemType.Submenu) throw new InvalidOperationException(
                $"Menu item {Name} is not a group.");

            item.Parent = this;
            _items.Add(item);
        }

        internal IEnumerable<MenuItem> Descendants()
            => Items.SelectMany(i => new[] { i }.Concat(i.Descendants()));

        /// <summary>
        /// Sorts the items of a group by their display names, then by their names.
        /// </summary>
        /// <param name="comparer">the comparer of the display names</param>
        internal void SortItems(StringComparer comparer)
        {
            if (ItemType != MenuItemType.Submenu) return;

            var sorted = _items.OrderBy(i => i.DisplayName ?? string.Empty, comparer)
                .ThenBy(i => i.Name, StringComparer.Ordinal).ToList();
            _items.Clear();
            _items.AddRange(sorted);
        }

        /// <summary>
        /// Drops the separators that separate nothing (first, last, or next to another separator), at any depth,
        /// e.g. around the plugins' group when the tenant has no plugins.
        /// </summary>
        internal void TrimSeparators()
        {
            if (ItemType != MenuItemType.Submenu) return;

            for (var i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].ItemType != MenuItemType.Separator) _items[i].TrimSeparators();
                else if (i == 0 || i == _items.Count - 1 || _items[i - 1].ItemType == MenuItemType.Separator)
                    _items.RemoveAt(i);
            }
        }

        /// <summary>
        /// Enables a group if any of its items is enabled, from the leaves up.
        /// </summary>
        internal void UpdateGroupState()
        {
            if (ItemType != MenuItemType.Submenu) return;

            foreach (var item in _items) item.UpdateGroupState();
            IsEnabled = _items.Any(i => i.ItemType != MenuItemType.Separator && i.IsEnabled);
        }

        #endregion
    }
}

using System;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// Builds a part of the main menu, fluently: groups, use case items and separators. The app builds with a
    /// <see cref="MenuBuilder"/>, a plugin with a <see cref="PluginMenuBuilder"/>, the GUI app with a
    /// <see cref="GuiMenuBuilder"/>.
    /// </summary>
    /// <remarks>
    /// <para>A group has a key, unique within the menu, and a text key, the key of its label in the resources of the
    /// part that creates it: two groups may share a label (e.g. Advanced), never a key.</para>
    /// <para>A use case item's title is the use case's menu title (its metadata).</para>
    /// </remarks>
    /// <typeparam name="TBuilder">the type of the builder</typeparam>
    public abstract class MenuBuilderBase<TBuilder> where TBuilder : MenuBuilderBase<TBuilder>
    {
        private protected MenuBuilderBase(MainMenu menu, MenuItem group, MenuPart part)
        {
            Menu = menu;
            CurrentGroup = group;
            Part = part;
        }


        private protected MainMenu Menu { get; }

        private protected MenuItem CurrentGroup { get; }

        private protected MenuPart Part { get; }


        /// <summary>
        /// Creates a group here.
        /// </summary>
        /// <param name="key">the key of the group, unique within the menu (a public constant of the menu part)</param>
        /// <param name="textKey">the key of the group's label in this part's resources</param>
        /// <param name="build">builds the items of the group</param>
        /// <param name="icon">the icon of the group, if any</param>
        /// <param name="openToPlugins">whether the tenant's plugins may append their own items to the group, after the
        /// app's (<see cref="IPluginMenu.Extend"/>); only the app opens its groups (<see cref="MenuBuilder"/>)</param>
        /// <returns>this builder, to go on at this level</returns>
        /// <exception cref="InvalidOperationException">the menu has a group with the key already; or a plugin or the
        /// GUI app opens a group to plugins</exception>
        public TBuilder Group(string key, string textKey, Action<TBuilder> build, string icon = null,
            bool openToPlugins = false)
        {
            if (openToPlugins && !Part.IsApp) throw new InvalidOperationException(
                $"Only the app opens its groups to plugins; {Part.Owner} cannot open group '{key}'.");

            var group = MenuItem.CreateGroup(Part.GroupKey(key), Part.Text(textKey), icon);

            Menu.RegisterGroup(group, Part.Owner, openToPlugins);

            Add(group);

            build?.Invoke(Nested(group));

            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a use case item.
        /// </summary>
        /// <typeparam name="TUseCase">the use case interface, remote or local</typeparam>
        /// <param name="icon">the icon of the item, if any</param>
        /// <returns>this builder</returns>
        public TBuilder Item<TUseCase>(string icon = null) where TUseCase : class => Item(typeof(TUseCase), icon);

        /// <summary>
        /// Adds a use case item.
        /// </summary>
        /// <param name="useCaseType">the use case interface, remote or local</param>
        /// <param name="icon">the icon of the item, if any</param>
        /// <returns>this builder</returns>
        public TBuilder Item(Type useCaseType, string icon = null)
        {
            Part.EnsureOwnUseCase(useCaseType, nameof(useCaseType));

            var metadata = Part.Metadata.GetUseCaseMetadata(useCaseType) ?? throw new InvalidOperationException(
                $"There is no metadata for use case {useCaseType.FullName}.");

            Add(MenuItem.CreateUseCase(useCaseType, metadata.GetMenuTitle, icon));

            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a separator to the group.
        /// </summary>
        /// <returns>this builder</returns>
        /// <exception cref="InvalidOperationException">at the top level</exception>
        public TBuilder Separator()
        {
            if (null == CurrentGroup) throw new InvalidOperationException("A separator can only be added to a group.");

            Add(MenuItem.CreateSeparator());

            return (TBuilder)this;
        }


        private protected abstract TBuilder Nested(MenuItem group);

        private protected void Add(MenuItem item)
        {
            if (null == CurrentGroup) Menu.AddTopItem(item);
            else CurrentGroup.Add(item);
        }
    }
}

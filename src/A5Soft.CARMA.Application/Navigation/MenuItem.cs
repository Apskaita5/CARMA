using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;

namespace A5Soft.CARMA.Application.Navigation
{
    [Serializable]
    public class MenuItem
    {
        #region Member Fields

        private readonly LocalizableString _displayName = new LocalizableString("Name");
        private readonly LocalizableString _description = new LocalizableString("Description");
        [NonSerialized]
        private Action<MenuItem> _onClick;
        [NonSerialized]
        private object _tag;

        #endregion

        #region Constructors

        /// <summary>
        /// overload to create a separator menu item
        /// </summary>
        private MenuItem()
        {
            Name = string.Empty;
            ResourceType = null;
            Icon = string.Empty;
            IsEnabled = false;
            UseCaseType = null;
            Items = null;
            IsSeparator = true;
            IsMenuGroup = false;
        }

        /// <summary>
        /// overload for common leaf menu item
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="DisplayName"/></param>
        /// <param name="description">a resource key string for <see cref="Description"/></param>
        /// <param name="resourceType"><see cref="System.Type"/> that contains the resources
        /// for <see cref="DisplayName"/> and <see cref="Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        /// <param name="useCaseType">A type of the (main) use case that handles action
        /// associated with the menu item. E.g. create invoice use case handles menu item "New Invoice".</param>
        private MenuItem(string name, string displayName, string description, Type resourceType,
            Type useCaseType, string icon = "")
        {
            if (displayName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(displayName));

            Name = name ?? string.Empty;
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            _displayName.Value = displayName;
            _description.Value = description ?? string.Empty;
            _displayName.ResourceType = resourceType;
            _description.ResourceType = resourceType;

            Icon = icon ?? string.Empty;
            IsEnabled = true;
            UseCaseType = useCaseType ?? throw new ArgumentNullException(nameof(useCaseType));
            Items = null;
            IsSeparator = false;
            IsMenuGroup = false;
        }

        /// <summary>
        /// overload for group menu item
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="DisplayName"/></param>
        /// <param name="description">a resource key string for <see cref="Description"/></param>
        /// <param name="resourceType"><see cref="System.Type"/> that contains the resources
        /// for <see cref="DisplayName"/> and <see cref="Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        private MenuItem(string name, string displayName, string description, Type resourceType,
            string icon = "")
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (displayName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(displayName));

            Name = name;
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            _displayName.Value = displayName;
            _description.Value = description ?? string.Empty;
            _displayName.ResourceType = resourceType;
            _description.ResourceType = resourceType;

            Icon = icon ?? string.Empty;
            IsEnabled = true;
            UseCaseType = null;
            Items = new List<MenuItem>();
            IsSeparator = false;
            IsMenuGroup = true;
        }

        /// <summary>
        /// overload for GUI app defined leaf menu item
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="DisplayName"/></param>
        /// <param name="description">a resource key string for <see cref="Description"/></param>
        /// <param name="resourceType"><see cref="System.Type"/> that contains the resources
        /// for <see cref="DisplayName"/> and <see cref="Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        /// <param name="onClick">an action that shall be executed when a user clicks the menu item</param>
        /// <param name="tag">an arbitrary object associated with the menu item.
        /// E.g. company info for login selector</param>
        private MenuItem(string name, string displayName, string description, Type resourceType,
            Action<MenuItem> onClick, object tag = null, string icon = "")
        {
            if (displayName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(displayName));

            Name = name;
            ResourceType = resourceType;
            _displayName.Value = displayName;
            _description.Value = description ?? string.Empty;
            _displayName.ResourceType = resourceType;
            _description.ResourceType = resourceType;

            Icon = icon ?? string.Empty;
            IsEnabled = true;
            UseCaseType = null;
            Items = null;
            IsSeparator = false;
            IsMenuGroup = false;
            OnClick = onClick;
        }

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets a name of the menu item (should be unique within main menu).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the DisplayName property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetDisplayName"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// name for display.
        /// <para>
        /// The <see cref="GetDisplayName"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// The name is generally used as the menu label for a UI element bound to the member. 
        /// </value>
        public string DisplayName
        {
            get
            {
                return this._displayName.Value;
            }
        }

        /// <summary>
        /// Gets the Description attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetDescription"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// description for display.
        /// <para>
        /// The <see cref="GetDescription"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// Description is generally used as a tool tip or description a UI element bound to the member
        /// </value>
        public string Description
        {
            get
            {
                return this._description.Value;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> that contains the resources for <see cref="DisplayName"/>
        /// and <see cref="Description"/>.
        /// Using <see cref="ResourceType"/> along with these Key properties, allows the <see cref="GetDisplayName"/>
        /// and <see cref="GetDescription"/> methods to return localized values.
        /// </summary>
        public Type ResourceType { get; }

        /// <summary>
        /// Icon of the menu item if exists. Optional.
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// Gets a value indicating whether the menu item can be invoked by the user.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Gets a type of the (main) use case that handles action associated with the menu item.
        /// E.g. create invoice use case handles menu item "New Invoice".
        /// </summary>
        public Type UseCaseType { get; }

        /// <summary>
        /// Sub items of this menu item. Optional.
        /// </summary>
        public virtual List<MenuItem> Items { get; private set; }

        /// <summary>
        /// Returns true if this menu item has no child <see cref="Items"/>.
        /// </summary>
        public bool IsLeaf 
            => null == Items || Items.Count < 1;

        /// <summary>
        /// Gets or sets an action that shall be executed when a user clicks the menu item.
        /// </summary>
        public Action<MenuItem> OnClick 
        { get => _onClick; set => _onClick = value; }

        /// <summary>
        /// Gets or sets an arbitrary object associated with the menu item.
        /// E.g. company info for login selector.
        /// </summary>
        public object Tag
        { get => _tag; set => _tag = value; }

        /// <summary>
        /// Gets a value indicating whether it's a menu group separator (not an actual menu item).
        /// </summary>
        public bool IsSeparator { get; }

        /// <summary>
        /// Gets a value indicating whether the menu item is intended to be a menu group (not an actual menu item).
        /// </summary>
        public bool IsMenuGroup { get; }

        #endregion

        #region Factory Methods

        internal static MenuItem CreateMainMenuTopGroup(string name, string displayName, string description,
            Type resourceType, string icon = "")
        {
            return new MenuItem(name, displayName, description, resourceType, icon);
        }

        internal static MenuItem CreateMainMenuTopItem(string name, string displayName,
            string description, Type resourceType, Type useCaseType, string icon = "")
        {
            return new MenuItem(name, displayName, description, resourceType, useCaseType, icon);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the UI display string for DisplayName.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="DisplayName"/> or the
        /// localized string found when <see cref="ResourceType"/> has been specified and <see cref="DisplayName"/>
        /// represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="DisplayName"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="DisplayName"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="DisplayName"/> property,
        /// but a public static property with a name matching the <see cref="DisplayName"/> value couldn't be found
        /// on the <see cref="ResourceType"/>.
        /// </exception>
        public string GetDisplayName()
        {
            return this._displayName.GetLocalizableValue();
        }

        /// <summary>
        /// Gets the UI display string for Description.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="Description"/> or the
        /// localized string found when <see cref="ResourceType"/> has been specified and <see cref="Description"/>
        /// represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="Description"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="Description"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="Description"/> property,
        /// but a public static property with a name matching the <see cref="Description"/> value couldn't be found
        /// on the <see cref="ResourceType"/>.
        /// </exception>
        public string GetDescription()
        {
            return this._description.GetLocalizableValue();
        }

        /// <summary>
        /// Invokes on click action for the menu item.
        /// Throws InvalidOperationException if <see cref="OnClick"/> handler is not set
        /// or it is a menu group item (not <see cref="IsLeaf"/>) or it <see cref="IsSeparator"/>.
        /// </summary>
        public void InvokeOnClick()
        {
            if (null == _onClick) throw new InvalidOperationException(
                $"OnClick method is not set for menu item {Name}.");
            if (!IsLeaf) throw new InvalidOperationException(
                $"OnClick method cannot be invoked for a menu group ({Name}).");
            if (IsSeparator) throw new InvalidOperationException(
                $"OnClick method cannot be invoked for a menu group separator.");

            _onClick(this);
        }

        /// <summary>
        /// Gets a value indicating whether it's possible to invoke <see cref="OnClick"/> handler,
        /// i.e. the menu item is not a separator, the menu item <see cref="IsLeaf"/>
        /// and the <see cref="OnClick"/> handler is set. 
        /// </summary>
        /// <returns>a value indicating whether it's possible to invoke <see cref="OnClick"/> handler,
        /// i.e. the menu item is not a separator, the menu item <see cref="IsLeaf"/>
        /// and the <see cref="OnClick"/> handler is set</returns>
        public bool CanInvokeOnClick()
        {
            return (IsLeaf && !IsSeparator && null != _onClick);
        }

        /// <summary>
        /// Adds a menu group separator to child items.
        /// </summary>
        public void AddSeparator()
        {
            if (!IsMenuGroup) throw new InvalidOperationException(
                $"Menu item {Name} is not a menu group.");
            
            if (null == Items) Items = new List<MenuItem>();

            Items.Add(new MenuItem());
        }

        /// <summary>
        /// Adds a menu group to child items.
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="DisplayName"/></param>
        /// <param name="description">a resource key string for <see cref="Description"/></param>
        /// <param name="resourceType"><see cref="System.Type"/> that contains the resources
        /// for <see cref="DisplayName"/> and <see cref="Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        public void AddMenuGroup(string name, string displayName, string description, Type resourceType,
            string icon = "")
        {
            if (!IsMenuGroup) throw new InvalidOperationException(
                $"Menu item {Name} is not a menu group.");

            if (null == Items) Items = new List<MenuItem>();

            Items.Add(new MenuItem(name, displayName, description, resourceType, icon));
        }

        /// <summary>
        /// Adds a leaf menu item to child items.
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="DisplayName"/></param>
        /// <param name="description">a resource key string for <see cref="Description"/></param>
        /// <param name="resourceType"><see cref="System.Type"/> that contains the resources
        /// for <see cref="DisplayName"/> and <see cref="Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        /// <param name="useCaseType">A type of the (main) use case that handles action
        /// associated with the menu item. E.g. create invoice use case handles menu item "New Invoice".</param>
        public void AddLeaf(string name, string displayName, string description, Type resourceType,
            Type useCaseType, string icon = "")
        {
            if (!IsMenuGroup) throw new InvalidOperationException(
                $"Menu item {Name} is not a menu group.");

            if (null == Items) Items = new List<MenuItem>();

            Items.Add(new MenuItem(name, displayName, description, resourceType, useCaseType, icon));
        }

        /// <summary>
        /// Adds a GUI app defined leaf menu item (e.g. app settings that are specific to winforms app).
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="DisplayName"/></param>
        /// <param name="description">a resource key string for <see cref="Description"/></param>
        /// <param name="resourceType"><see cref="System.Type"/> that contains the resources
        /// for <see cref="DisplayName"/> and <see cref="Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        /// <param name="onClick">an action that shall be executed when a user clicks the menu item</param>
        /// <param name="tag">an arbitrary object associated with the menu item.
        /// E.g. company info for login selector</param>
        public void AddGuiAppDefinedLeaf(string name, string displayName, string description, 
            Type resourceType, Action<MenuItem> onClick, object tag = null, string icon = "")
        {
            if (!IsMenuGroup) throw new InvalidOperationException(
                $"Menu item {Name} is not a menu group.");

            if (null == Items) Items = new List<MenuItem>();

            Items.Add(new MenuItem(name, displayName, description, resourceType, onClick, tag, icon));
        }

        internal MenuItem GetMenuGroup(string name)
        {
            if (!IsMenuGroup) return null;

            if (Name.Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                return this;

            MenuItem result = null;
            foreach (var item in Items)
            {
                result = item.GetMenuGroup(name);
                if (!result.IsNull()) return result;
            }

            return null;
        }

        internal void SetAuthorization(IAuthorizationProvider authorizationProvider, ClaimsIdentity user)
        {
            if (IsLeaf)
            {
                if (null == UseCaseType) IsEnabled = true;
                else
                {
                    var authorizer = authorizationProvider.GetAuthorizer(UseCaseType);
                    IsEnabled = authorizer.IsAuthorized(user);
                }
            }
            else
            {
                foreach (var item in Items)
                {
                    item.SetAuthorization(authorizationProvider, user);
                }

                IsEnabled = Items.Any(i => i.IsEnabled);
            }
        }

        internal void AddPluginMenuItem(MenuItem pluginMenuItem)
        {
            if (!IsMenuGroup) throw new InvalidOperationException(
                $"Menu item {Name} is not a menu group.");

            if (null == Items) Items = new List<MenuItem>();

            Items.Add(pluginMenuItem);
        }

        #endregion

    }
}

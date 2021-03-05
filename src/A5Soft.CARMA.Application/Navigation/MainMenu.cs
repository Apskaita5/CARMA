using A5Soft.CARMA.Application.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Navigation
{
    [Serializable]
    public class MainMenu
    {

        public List<MenuItem> TopItems { get; } = new List<MenuItem>();


        /// <summary>
        /// Adds a top menu group.
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="MenuItem.DisplayName"/></param>
        /// <param name="description">a resource key string for <see cref="MenuItem.Description"/></param>
        /// <param name="resourceType"><see cref="Type"/> that contains the resources
        /// for <see cref="MenuItem.DisplayName"/> and <see cref="MenuItem.Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        public void AddMainMenuTopGroup(string name, string displayName, string description,
            Type resourceType, string icon = "")
        {
            TopItems.Add(MenuItem.CreateMainMenuTopGroup(name, displayName, description, resourceType, icon));
        }

        /// <summary>
        /// Adds a top leaf menu item.
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="MenuItem.DisplayName"/></param>
        /// <param name="description">a resource key string for <see cref="MenuItem.Description"/></param>
        /// <param name="resourceType"><see cref="Type"/> that contains the resources
        /// for <see cref="MenuItem.DisplayName"/> and <see cref="MenuItem.Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        /// <param name="useCaseType">A type of the (main) use case that handles action
        /// associated with the menu item. E.g. create invoice use case handles menu item "New Invoice".</param>
        public void AddMainMenuTopItem(string name, string displayName,
            string description, Type resourceType, Type useCaseType, string icon = "")
        {
            TopItems.Add(MenuItem.CreateMainMenuTopItem(name, displayName, description, 
                resourceType, useCaseType, icon));
        }

        public MenuItem GetMenuGroup(string name)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            name = name.Trim();

            MenuItem result = null;
            foreach (var topItem in TopItems)
            {
                result = topItem.GetMenuGroup(name);
                if (!result.IsNull()) return result;
            }

            return null;
        }

        internal void SetAuthorization(IAuthorizationProvider authorizationProvider, ClaimsIdentity user)
        {
            foreach (var item in TopItems)
            {
                item.SetAuthorization(authorizationProvider, user);
            }
        }

    }
}

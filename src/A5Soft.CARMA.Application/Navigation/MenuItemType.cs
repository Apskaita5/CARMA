
namespace A5Soft.CARMA.Application.Navigation
{
    public enum MenuItemType
    {
        /// <summary>
        /// a collection of child menu items
        /// </summary>
        Submenu,

        /// <summary>
        /// a graphical separator for menu items within a single (sub)menu
        /// </summary>
        Separator,

        /// <summary>
        /// a menu item that has an associated use case interface type
        /// </summary>
        UseCase,

        /// <summary>
        /// a menu item added by a GUI app (e.g. settings for winforms app)
        /// </summary>
        GuiAppItem
    }
}

using System;
using System.Collections.ObjectModel;

namespace A5Soft.CARMA.Domain.Metadata
{
    /// <summary>
    /// Common interface for entity (class) metadata localization provider.
    /// </summary>
    /// <remarks>Entity could be commonly described in:
    /// 1) Web edit form header or winforms/WPF form caption for a new entity instance;
    /// 2) Web edit form header or winforms/WPF form caption for an existing entity instance being edited;
    /// 3) Menu item for a new instance of the entity or a tooltip for a button (in all types of interfaces);
    /// 4) (Contextual) menu item for editing an entity or a tooltip for a button (in all types of interfaces);
    /// 5) (Contextual) menu item for deleting an entity or a tooltip for a button (in all types of interfaces).
    /// Paras 4 and 5 are actually actions (use cases), therefore not a part of entity metadata.
    /// While creating a new instance (para 3) of an entity usually is not a use case
    /// (does not require specific actions except for invoking constructor).</remarks>
    public interface IEntityMetadata
    {
        /// <summary>
        /// Gets a type of the entity described.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Gets a collection of metadata descriptions for entity properties.
        /// </summary>
        ReadOnlyDictionary<string, IPropertyMetadata> Properties { get; }

        /// <summary>
        /// Gets an array of all the property names.
        /// </summary>
        string[] PropertyNames { get; }

        /// <summary>
        /// Gets a collection of metadata descriptions for entity methods.
        /// </summary>
        ReadOnlyDictionary<string, IMethodMetadata> Methods { get; }


        /// <summary>
        /// Gets a <see cref="IPropertyMetadata"/> instance for the property specified
        /// (null if no metadata for the property).
        /// </summary>
        /// <param name="propertyName">a name of the property to get the metadata for</param>
        /// <returns>a <see cref="IPropertyMetadata"/> instance for the property specified
        /// (null if no metadata for the property)</returns>
        IPropertyMetadata GetPropertyMetadata(string propertyName);

        /// <summary>
        /// Gets a localized value that is used for display in the UI
        /// for a new instance of the entity (typically used as a form caption/header).
        /// </summary>
        /// <returns>a localized value that is used for display in the UI</returns>
        string GetDisplayNameForNew();

        /// <summary>
        /// Gets a localized value that is used to display a description in the UI
        /// for an old entity instance (typically used as a form caption/header).
        /// </summary>
        /// <returns>a localized value that is used for display in the UI</returns>
        string GetDisplayNameForOld();

        /// <summary>
        /// Gets a localized value that is used to display a description in the UI
        /// for a menu item or button (tooltip) that create a new instance of the entity.
        /// </summary>
        /// <returns>a localized value that is used for display in the UI</returns>
        string GetDisplayNameForCreateNew();

        /// <summary>
        /// Gets an URI for help file and topic for the entity.
        /// </summary>
        string GetHelpUri();
    }
}

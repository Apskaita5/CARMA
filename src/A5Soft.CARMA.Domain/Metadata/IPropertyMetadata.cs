using System;

namespace A5Soft.CARMA.Domain.Metadata
{
    /// <summary>
    /// Common interface for property metadata localization provider.
    /// </summary>
    public interface IPropertyMetadata
    {

        /// <summary>
        /// Gets the type of the entity that the property belongs to.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Gets the name of this property.
        /// </summary>
        string Name { get; }
               
        /// <summary>
        /// Gets a value that indicates whether the property has a public setter.  
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets the type of this property (value).
        /// </summary>
        Type PropertyType { get; }


        /// <summary>
        /// Gets a localized value that is used for display in the UI (typically used as the field label).
        /// </summary>
        /// <returns>a localized value that is used for display in the UI</returns>
        string GetDisplayName();

        /// <summary>
        /// Gets a localized value that is used for the grid column label.
        /// </summary>
        /// <returns>a localized value that is used for the grid column label</returns>
        string GetDisplayShortName();

        /// <summary>
        /// Gets a localized value that is used to display a description in the UI
        /// (typically used as a tooltip or description UI element).
        /// </summary>
        /// <returns>a localized value that is used to display a description in the UI</returns>
        string GetDisplayDescription();

        /// <summary>
        /// Gets a localized value that will be used to set the watermark for prompts in the UI.
        /// </summary>
        /// <returns>a localized value that will be used to set the watermark for prompts in the UI</returns>
        string GetDisplayPrompt();

        /// <summary>
        /// Gets a localized value that is used to group fields in the UI.
        /// </summary>
        /// <returns>a localized value that is used to group fields in the UI</returns>
        string GetDisplayGroupName();

        /// <summary>
        /// Gets the order weight of the column (field).
        /// </summary>
        /// <returns></returns>
        /// <remarks>Columns (fields) are sorted in increasing order based on the order value.
        /// Columns (fields) without this DisplayAttribute (or without DisplayAttribute.Order value set)
        /// have an order value of 10000. This value lets explicitly-ordered fields be displayed before
        /// and after the fields that do not have a specified order.
        /// Negative values are valid and can be used to position a column before all non-negative columns.</remarks>
        int GetDisplayOrder();

        /// <summary>
        /// Gets a value that indicates whether UI should be generated automatically in order to display this field.
        /// </summary>
        /// <param name="defaultValue">a default value if the property does not have a DisplayAttribute
        /// or DisplayAttribute.AutoGenerateField is not set</param>
        /// <returns>a value that indicates whether UI should be generated automatically in order to display this field</returns>
        bool GetDisplayAutoGenerate(bool defaultValue = true);

        /// <summary>
        /// Gets a value that indicates whether filtering UI is automatically displayed for this field.
        /// </summary>
        /// <param name="defaultValue">a default value if the property does not have a DisplayAttribute
        /// or DisplayAttribute.AutoGenerateFilter is not set</param>
        /// <returns>a value that indicates whether filtering UI is automatically displayed for this field</returns>
        bool GetAutoGenerateFilter(bool defaultValue = true);

        /// <summary>
        /// Gets a property value for the entity instance.
        /// </summary>
        /// <param name="instance">the entity instance to get a property value for</param>
        /// <returns>a property value for the entity instance</returns>
        object GetValue(object instance);

    }
}

using System;

namespace A5Soft.CARMA.Domain.Metadata
{
    /// <summary>
    /// Common interface for entity (class) method metadata localization provider.
    /// </summary>
    /// <remarks>Entity methods could be commonly described in the tooltips of respective buttons.
    /// The action buttons themselves are usually visualized as icons for both
    /// desktop and web apps.</remarks>
    public interface IMethodMetadata
    {
        /// <summary>
        /// Gets the type of the entity that the method belongs to.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Gets the name of this method.
        /// </summary>
        string Name { get; }
            
        /// <summary>
        /// Gets a localized value that is used for display in the UI
        /// (typically used as button tooltips).
        /// </summary>
        /// <returns>a localized value that is used for display in the UI</returns>
        string GetDisplayName();
    }
}

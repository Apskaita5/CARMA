using System;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Marker attribute for (technical) properties that shall not be included within the entity metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IgnorePropertyMetadataAttribute : Attribute
    {
        
    }
}

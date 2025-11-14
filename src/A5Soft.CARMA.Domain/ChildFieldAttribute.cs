using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// An attribute to mark a child field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ChildFieldAttribute : Attribute
    {
        /// <summary>
        /// A child field attribute.
        /// </summary>
        /// <param name="propertyName">a name of the property that exposes the field</param>
        public ChildFieldAttribute(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            PropertyName = propertyName.Trim();
        }

        /// <summary>
        /// a name of the property that exposes the field
        /// </summary>
        public string PropertyName { get; }
    }
}

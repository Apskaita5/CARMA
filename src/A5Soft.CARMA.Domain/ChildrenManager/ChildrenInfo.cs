using System.Collections.Generic;
using System.Reflection;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Defines a generic class storing information related to child fields of type TClass.
    /// </summary>
    internal sealed class ChildrenInfo<TClass>
        where TClass : DomainObject<TClass>
    {
        /// <summary>
        /// The default constructor for the ChildrenInfo class. It initializes a list of ChildFieldInfo objects.
        public ChildrenInfo()
        {
            var result = new List<ChildFieldInfo<TClass>>();
            foreach (var field in typeof(TClass).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (field.FieldType.IsClass)
                {
                    var attribute = field.GetCustomAttribute<ChildFieldAttribute>();
                    if (null != attribute)
                    {
                        result.Add(new ChildFieldInfo<TClass>(field, attribute.PropertyName));
                    }
                }
            }
            ChildFields = result;
        }

        /// <summary>
        /// Gets the read-only collection of child fields for a class of type TClass.
        /// </summary>
        public IReadOnlyCollection<ChildFieldInfo<TClass>> ChildFields { get; }
    }
}
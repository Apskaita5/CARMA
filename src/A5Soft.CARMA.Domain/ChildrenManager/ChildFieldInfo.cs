using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Represents a collection of information for a child field as well as compiled lambda expressions for access.
    internal sealed class ChildFieldInfo<TClass>
        where TClass : DomainObject<TClass>
    {
        /// <summary>
        /// Constructs a new instance of the ChildFieldInfo class with specified field and property name.
        /// </summary>
        /// <param name="field">Represents information about a field of a class.</param>
        /// <param name="propertyName">The name of the property that exposes the field.</param>
        /// <returns>
        /// A new instance of the ChildFieldInfo class initialized with specified parameters.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided field or propertyName is null.</exception>
        public ChildFieldInfo(FieldInfo field, string propertyName)
        {
            if (null == field) throw new ArgumentNullException(nameof(field));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            PropertyName = propertyName;
            FieldName = field.Name;
            FieldType = field.FieldType;
            Getter = CreateGetter(field);
            IsBindable = typeof(IBindable).IsAssignableFrom(FieldType);
            IsStateful = typeof(ITrackState).IsAssignableFrom(FieldType);
            IsINotifyPropertyChanged = typeof(INotifyPropertyChanged).IsAssignableFrom(FieldType);
            IsIBindingList = typeof(IBindingList).IsAssignableFrom(FieldType);
            IsINotifyCollectionChanged = typeof(INotifyCollectionChanged).IsAssignableFrom(FieldType);
            IsINotifyChildChanged = typeof(INotifyChildChanged).IsAssignableFrom(FieldType);
        }


        /// <summary>
        /// Gets the value of the property that exposes the field.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        public Type FieldType { get; }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        ///  Gets a value indicating whether the child value is <see cref="IBindable"/>.
        /// </summary>
        public bool IsBindable { get; }

        /// <summary>
        /// Gets a value indicating whether the child value is <see cref="ITrackState"/>.
        /// </summary>
        public bool IsStateful { get; }

        /// <summary>
        /// Gets a value indicating whether the child value implements the INotifyPropertyChanged interface.
        /// </summary>
        public bool IsINotifyPropertyChanged { get; }

        /// <summary>
        /// Gets a value indicating whether the child value implements the IBindingList interface.
        /// </summary>
        public bool IsIBindingList { get; }

        /// <summary>
        /// Gets a value indicating whether the child value implements the INotifyCollectionChanged interface.
        /// </summary>
        public bool IsINotifyCollectionChanged { get; }

        /// <summary>
        /// Gets a value indicating whether the child value is <see cref="INotifyChildChanged"/>.
        /// </summary>
        public bool IsINotifyChildChanged { get; }

        /// <summary>
        /// Gets a function that fetches the value of the field from an instance of TClass.
        /// </summary>
        public Func<TClass, object> Getter { get; }

        
        /// <summary>
        /// Creates a function to get the value from the specified field of an instance of the class TClass.
        /// </summary>
        /// <param name="fieldInfo">The metadata for the field to be accessed.</param>
        /// <returns>
        /// A function that takes an instance of TClass and returns the value of the specified field as an object.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the `fieldInfo` parameter is null.</exception>
        private static Func<TClass, object> CreateGetter(FieldInfo fieldInfo)
        {
            if (null == fieldInfo) throw new ArgumentNullException(nameof(fieldInfo));

            // Parameter: the object instance
            var instanceParam = Expression.Parameter(typeof(TClass), "instance");

            // Access the field
            var fieldAccess = Expression.Field(instanceParam, fieldInfo);

            // Convert the field value to object
            var fieldValueAsObject = Expression.Convert(fieldAccess, typeof(object));

            // Compile the expression
            var lambda = Expression.Lambda<Func<TClass, object>>(
                fieldValueAsObject,
                instanceParam
            );

            return lambda.Compile();
        }
    }
}
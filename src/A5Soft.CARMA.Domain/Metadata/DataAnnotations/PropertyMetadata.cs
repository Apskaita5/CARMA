using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using A5Soft.CARMA.Domain.Reflection;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Default implementation of IPropertyMetadata using DisplayAttribute.
    /// </summary>
    internal class PropertyMetadata : IPropertyMetadata
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly DisplayAttribute _displayAttribute;
        private readonly Func<object, object> _getter;


        internal PropertyMetadata(PropertyInfo propInfo)
        {
            _propertyInfo = propInfo ?? throw new ArgumentNullException(nameof(propInfo));
            IsReadOnly = (null == propInfo.GetSetMethod(false));
            _displayAttribute = propInfo.GetCustomAttributeWithInheritance<DisplayAttribute>();
            _getter = CreateGetter(propInfo);
        }


        /// <inheritdoc cref="IPropertyMetadata.EntityType" />
        public Type EntityType => _propertyInfo.DeclaringType;

        /// <inheritdoc cref="IPropertyMetadata.Name" />
        public string Name => _propertyInfo.Name;

        /// <inheritdoc cref="IPropertyMetadata.IsReadOnly" />
        public bool IsReadOnly { get; }

        /// <inheritdoc cref="IPropertyMetadata.PropertyType" />
        public Type PropertyType => _propertyInfo.PropertyType;


        /// <inheritdoc cref="IPropertyMetadata.GetDisplayName" />
        public string GetDisplayName()
        {
            if (_displayAttribute.IsNull()) return Name.SplitCamelCase();

            var value = _displayAttribute.GetName();
            return value.IsNullOrWhiteSpace() ? Name.SplitCamelCase() : value;
        }

        /// <inheritdoc cref="IPropertyMetadata.GetDisplayShortName" />
        public string GetDisplayShortName()
        {
            if (_displayAttribute.IsNull()) return Name.SplitCamelCase();

            var value = _displayAttribute.GetShortName();
            return value.IsNullOrWhiteSpace() ? Name.SplitCamelCase() : value;
        }

        /// <inheritdoc cref="IPropertyMetadata.GetDisplayDescription" />
        public string GetDisplayDescription()
        {
            if (_displayAttribute.IsNull()) return string.Empty;

            return _displayAttribute.GetDescription() ?? string.Empty;
        }

        /// <inheritdoc cref="IPropertyMetadata.GetDisplayPrompt" />
        public string GetDisplayPrompt()
        {
            if (_displayAttribute.IsNull()) return string.Empty;

            return _displayAttribute.GetPrompt() ?? string.Empty;
        }

        /// <inheritdoc cref="IPropertyMetadata.GetDisplayGroupName" />
        public string GetDisplayGroupName()
        {
            if (_displayAttribute.IsNull()) return string.Empty;

            return _displayAttribute.GetGroupName() ?? string.Empty;
        }

        /// <inheritdoc cref="IPropertyMetadata.GetDisplayOrder" />
        public int GetDisplayOrder()
        {
            if (_displayAttribute.IsNull()) return -1;

            return _displayAttribute.GetOrder() ?? 100000;
        }

        /// <inheritdoc cref="IPropertyMetadata.GetDisplayAutoGenerate" />
        public bool GetDisplayAutoGenerate(bool defaultValue = true)
        {
            if (_displayAttribute.IsNull()) return defaultValue;

            return _displayAttribute.GetAutoGenerateField() ?? defaultValue;
        }

        /// <inheritdoc cref="IPropertyMetadata.GetAutoGenerateFilter" />
        public bool GetAutoGenerateFilter(bool defaultValue = true)
        {
            if (_displayAttribute.IsNull()) return defaultValue;

            return _displayAttribute.GetAutoGenerateFilter() ?? defaultValue;
        }

        /// <inheritdoc cref="IPropertyMetadata.GetValue" />
        public object GetValue(object instance)
        {
            if (object.ReferenceEquals(instance, null)) throw new ArgumentNullException(nameof(instance));

            if (!EntityType.IsAssignableFrom(instance.GetType())) throw new ArgumentException(
                $"Instance of type {instance.GetType().FullName} is not assignable to entity type {EntityType.FullName}.",
                    nameof(instance));

            return _getter(instance);
        }


        private static Func<object, object> CreateGetter(PropertyInfo propertyInfo)
        {
            // Parameter: (object instance)
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            // Convert instance to declaring type
            var typedInstance = Expression.Convert(instanceParam, propertyInfo.DeclaringType);
            // Access property
            var propertyAccess = Expression.Property(typedInstance, propertyInfo);
            // Box value type -> object
            var boxed = Expression.Convert(propertyAccess, typeof(object));
            // Compile lambda: (object instance) => (object)instance.Property
            return Expression.Lambda<Func<object, object>>(boxed, instanceParam).Compile();
        }
    }
}

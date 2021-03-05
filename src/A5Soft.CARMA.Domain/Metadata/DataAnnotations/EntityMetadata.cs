using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Domain.Reflection;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    ///  a metadata description for entity type using DataAnnotations attributes
    /// </summary>
    internal class EntityMetadata : IEntityMetadata
    {
        #region Fields

        private readonly ClassDescriptionAttribute _displayAttribute = null;

        #endregion

        #region Initialization

        internal EntityMetadata(Type entityType)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));
            if (!entityType.IsClass && !entityType.IsInterface) throw new ArgumentException(
                "Metadata can only be defined for classes or interfaces.", nameof(entityType));

            EntityType = entityType;
            _displayAttribute = GetDescriptionAttribute(entityType);

            var relevantProperties = entityType.GetProperties()
                .Where(p => null == p.GetCustomAttributeWithInheritance<IgnorePropertyMetadataAttribute>())
                .ToList();

            Properties = new ReadOnlyDictionary<string, IPropertyMetadata>(
                relevantProperties.ToDictionary(k => k.Name, 
                    v => new PropertyMetadata(v) as IPropertyMetadata));

            PropertyNames = relevantProperties.Select(p => p.Name).ToArray();

            var relevantMethods = entityType.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public)
                .Select(m => (Method: m, 
                    Attribute: m.GetCustomAttributes<MethodDescriptionAttribute>(true)
                        .FirstOrDefault()))
                .Where(m => null != m.Attribute)
                .ToDictionary(k => k.Method.Name, 
                    v => new MethodMetadata(entityType, 
                        v.Method.Name, v.Attribute) as IMethodMetadata);

            Methods = new ReadOnlyDictionary<string, IMethodMetadata>(relevantMethods);
        }

        private static ClassDescriptionAttribute GetDescriptionAttribute(Type entityType)
        {
            var displayAttributes = entityType.GetCustomAttributes(typeof(ClassDescriptionAttribute), false);
            if ((null == displayAttributes || displayAttributes.Length < 1) && entityType.IsClass)
            {
                foreach (Type entityInterface in entityType.GetInterfaces()
                    .Where(t => typeof(IDomainObject).IsAssignableFrom(t)))
                {
                    displayAttributes = entityInterface.GetCustomAttributes(typeof(ClassDescriptionAttribute), false);
                    if (null != displayAttributes && displayAttributes.Length > 0) break;
                }
            }

            if (null != displayAttributes && displayAttributes.Length > 0)
                return (ClassDescriptionAttribute)displayAttributes[0];

            return null;
        }

        #endregion

        #region Properties

        /// <inheritdoc cref="IEntityMetadata.EntityType" />
        public Type EntityType { get; }

        /// <inheritdoc cref="IEntityMetadata.Properties" />
        public ReadOnlyDictionary<string, IPropertyMetadata> Properties { get; }

        /// <inheritdoc cref="IEntityMetadata.PropertyNames" />
        public string[] PropertyNames { get; }

        /// <inheritdoc cref="IEntityMetadata.Methods" />
        public ReadOnlyDictionary<string, IMethodMetadata> Methods { get; }

        #endregion

        #region IEntityMetadata Methods

        /// <inheritdoc cref="IEntityMetadata.GetDisplayNameForNew" />
        public string GetDisplayNameForNew()
        {
            if (_displayAttribute.IsNull()) return EntityType.Name.SplitCamelCase();

            var value = _displayAttribute.GetNameForNew();
            return value.IsNullOrWhiteSpace() ? EntityType.Name.SplitCamelCase() : value;
        }

        /// <inheritdoc cref="IEntityMetadata.GetDisplayNameForOld" />
        public string GetDisplayNameForOld()
        {
            if (_displayAttribute.IsNull()) return string.Empty;

            var value = _displayAttribute.GetNameForOld();
            return value.IsNullOrWhiteSpace() ? EntityType.Name.SplitCamelCase() : value;
        }

        /// <inheritdoc cref="IEntityMetadata.GetDisplayNameForCreateNew" />
        public string GetDisplayNameForCreateNew()
        {
            if (_displayAttribute.IsNull()) return string.Empty;

            return _displayAttribute.GetNameForCreateNew() ?? string.Empty;
        }

        /// <inheritdoc cref="IEntityMetadata.GetHelpUri" />
        public string GetHelpUri()
        {
            if (_displayAttribute.IsNull()) return string.Empty;

            return _displayAttribute.GetHelpUri() ?? string.Empty;
        }

        #endregion
    }
}

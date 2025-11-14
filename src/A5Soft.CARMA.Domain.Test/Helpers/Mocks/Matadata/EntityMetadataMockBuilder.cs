using A5Soft.CARMA.Domain.Metadata;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Matadata
{
    /// <summary>
    /// Fluent builder for creating IEntityMetadata mocks.
    /// </summary>
    public class EntityMetadataMockBuilder
    {
        private readonly Mock<IEntityMetadata> _mock;
        private Type _entityType;
        private readonly Dictionary<string, IPropertyMetadata> _properties = new();
        private readonly Dictionary<string, IMethodMetadata> _methods = new();
        private string _displayNameForNew;
        private string _displayNameForOld;
        private string _helpUri;

        public EntityMetadataMockBuilder(Type entityType)
        {
            _mock = new Mock<IEntityMetadata>();
            _entityType = entityType;
            _displayNameForNew = $"New {entityType.Name}";
            _displayNameForOld = $"Edit {entityType.Name}";
        }

        public EntityMetadataMockBuilder ForType<T>()
        {
            _entityType = typeof(T);
            _displayNameForNew = $"New {typeof(T).Name}";
            _displayNameForOld = $"Edit {typeof(T).Name}";
            return this;
        }

        public EntityMetadataMockBuilder WithProperty(IPropertyMetadata propertyMetadata)
        {
            _properties[propertyMetadata.Name] = propertyMetadata;
            return this;
        }

        public EntityMetadataMockBuilder WithProperty(
            string propertyName,
            Action<PropertyMetadataMockBuilder> configure = null)
        {
            var builder = new PropertyMetadataMockBuilder(propertyName)
                .ForEntityType(_entityType);

            configure?.Invoke(builder);

            _properties[propertyName] = builder.Build();
            return this;
        }

        public EntityMetadataMockBuilder WithMethod(IMethodMetadata methodMetadata)
        {
            _methods[methodMetadata.Name] = methodMetadata;
            return this;
        }

        public EntityMetadataMockBuilder WithMethod(
            string methodName,
            Action<MethodMetadataMockBuilder> configure = null)
        {
            var builder = new MethodMetadataMockBuilder(methodName)
                .ForEntityType(_entityType);

            configure?.Invoke(builder);

            _methods[methodName] = builder.Build();
            return this;
        }

        public EntityMetadataMockBuilder WithDisplayNameForNew(string displayName)
        {
            _displayNameForNew = displayName;
            return this;
        }

        public EntityMetadataMockBuilder WithDisplayNameForOld(string displayName)
        {
            _displayNameForOld = displayName;
            return this;
        }

        public EntityMetadataMockBuilder WithHelpUri(string helpUri)
        {
            _helpUri = helpUri;
            return this;
        }

        public IEntityMetadata Build()
        {
            var propertiesReadOnly = new ReadOnlyDictionary<string, IPropertyMetadata>(_properties);
            var methodsReadOnly = new ReadOnlyDictionary<string, IMethodMetadata>(_methods);

            _mock.Setup(x => x.EntityType).Returns(_entityType);
            _mock.Setup(x => x.Properties).Returns(propertiesReadOnly);
            _mock.Setup(x => x.PropertyNames).Returns(_properties.Keys.ToArray());
            _mock.Setup(x => x.Methods).Returns(methodsReadOnly);

            _mock.Setup(x => x.GetPropertyMetadata(It.IsAny<string>()))
                .Returns<string>(name => _properties.TryGetValue(name, out var prop) ? prop : null);

            _mock.Setup(x => x.GetDisplayNameForNew()).Returns(_displayNameForNew);
            _mock.Setup(x => x.GetDisplayNameForOld()).Returns(_displayNameForOld);
            _mock.Setup(x => x.GetHelpUri()).Returns(_helpUri ?? string.Empty);

            return _mock.Object;
        }

        //public static implicit operator IEntityMetadata(EntityMetadataMockBuilder builder)
        //    => builder.Build();
    }
}

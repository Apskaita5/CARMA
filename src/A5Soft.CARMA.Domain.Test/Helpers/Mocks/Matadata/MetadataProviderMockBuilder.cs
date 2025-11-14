using A5Soft.CARMA.Domain.Metadata;
using Moq;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Matadata
{
    /// <summary>
    /// Fluent builder for creating IMetadataProvider mocks.
    /// </summary>
    public class MetadataProviderMockBuilder
    {
        private readonly Mock<IMetadataProvider> _mock;
        private readonly Dictionary<Type, IEntityMetadata> _entityMetadata = new();
        private readonly Dictionary<Type, IUseCaseMetadata> _useCaseMetadata = new();

        public MetadataProviderMockBuilder()
        {
            _mock = new Mock<IMetadataProvider>();
        }

        public MetadataProviderMockBuilder WithEntityMetadata(
            Type entityType,
            IEntityMetadata metadata)
        {
            _entityMetadata[entityType] = metadata;
            return this;
        }

        public MetadataProviderMockBuilder WithEntityMetadata<T>(
            Action<EntityMetadataMockBuilder> configure = null)
        {
            var builder = new EntityMetadataMockBuilder(typeof(T));
            configure?.Invoke(builder);
            _entityMetadata[typeof(T)] = builder.Build();
            return this;
        }

        public MetadataProviderMockBuilder WithUseCaseMetadata(
            Type useCaseType,
            IUseCaseMetadata metadata)
        {
            _useCaseMetadata[useCaseType] = metadata;
            return this;
        }

        public MetadataProviderMockBuilder WithUseCaseMetadata<T>(
            Action<UseCaseMetadataMockBuilder> configure = null)
        {
            var builder = new UseCaseMetadataMockBuilder(typeof(T));
            configure?.Invoke(builder);
            _useCaseMetadata[typeof(T)] = builder.Build();
            return this;
        }

        public IMetadataProvider Build()
        {
            _mock.Setup(x => x.GetEntityMetadata(It.IsAny<Type>()))
                .Returns<Type>(type => _entityMetadata.TryGetValue(type, out var metadata)
                    ? metadata
                    : null);

            _mock.Setup(x => x.GetEntityMetadata<It.IsAnyType>())
                .Returns((Type type) => _entityMetadata.TryGetValue(type, out var metadata)
                    ? metadata
                    : null);

            _mock.Setup(x => x.GetUseCaseMetadata(It.IsAny<Type>()))
                .Returns<Type>(type => _useCaseMetadata.TryGetValue(type, out var metadata)
                    ? metadata
                    : null);

            _mock.Setup(x => x.GetUseCaseMetadata<It.IsAnyType>())
                .Returns((Type type) => _useCaseMetadata.TryGetValue(type, out var metadata)
                    ? metadata
                    : null);

            return _mock.Object;
        }

        //public static implicit operator IMetadataProvider(MetadataProviderMockBuilder builder)
        //    => builder.Build();
    }
}

using A5Soft.CARMA.Domain.Metadata;
using Moq;
using System;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Matadata
{
    /// <summary>
    /// Fluent builder for creating IMethodMetadata mocks.
    /// </summary>
    public class MethodMetadataMockBuilder
    {
        private readonly Mock<IMethodMetadata> _mock;
        private Type _entityType;
        private string _name;
        private string _displayName;

        public MethodMetadataMockBuilder(string methodName)
        {
            _mock = new Mock<IMethodMetadata>();
            _name = methodName;
            _displayName = methodName;
        }

        public MethodMetadataMockBuilder ForEntityType(Type entityType)
        {
            _entityType = entityType;
            return this;
        }

        public MethodMetadataMockBuilder ForEntityType<T>()
        {
            _entityType = typeof(T);
            return this;
        }

        public MethodMetadataMockBuilder WithDisplayName(string displayName)
        {
            _displayName = displayName;
            return this;
        }

        public IMethodMetadata Build()
        {
            _mock.Setup(x => x.EntityType).Returns(_entityType);
            _mock.Setup(x => x.Name).Returns(_name);
            _mock.Setup(x => x.GetDisplayName()).Returns(_displayName);

            return _mock.Object;
        }

        //public static implicit operator IMethodMetadata(MethodMetadataMockBuilder builder)
        //    => builder.Build();
    }
}

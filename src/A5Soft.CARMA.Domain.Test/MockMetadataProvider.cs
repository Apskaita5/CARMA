using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Test
{
    class MockMetadataProvider : IMetadataProvider
    {
        public IEntityMetadata GetEntityMetadata(Type entityType)
        {
            return new MockEntityMetadata(entityType);
        }

        public IEntityMetadata GetEntityMetadata<T>()
        {
            return new MockEntityMetadata(typeof(T));
        }

        public IUseCaseMetadata GetUseCaseMetadata(Type useCaseType)
        {
            throw new NotImplementedException();
        }

        public IUseCaseMetadata GetUseCaseMetadata<T>()
        {
            throw new NotImplementedException();
        }
    }
}

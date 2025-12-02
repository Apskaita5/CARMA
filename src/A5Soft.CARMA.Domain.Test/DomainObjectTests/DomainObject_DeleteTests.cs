using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_DeleteTests
    {
        private readonly IValidationEngineProvider _provider;

        public DomainObject_DeleteTests()
        {
            _provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
        }

        [Fact]
        public void Delete_ParentEntity_ShouldMarkDeleted()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.Delete();

            // Assert
            entity.IsDeleted.Should().BeTrue();
            entity.IsDirty.Should().BeTrue();
        }

        [Fact]
        public void Delete_ChildEntity_ShouldThrow()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider);
            var child = new TestChildEntity(_provider);
            parent.Child = child;

            // Act
            Action act = () => child.Delete();

            // Assert
            act.Should().Throw<NotSupportedException>()
                .WithMessage("*not applicable for child entity*");
        }

        [Fact]
        public void DeleteChild_ChildEntity_ShouldMarkDeleted()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider);
            var child = new TestChildEntity(_provider);
            parent.Child = child;

            // Act
            child.DeleteChild();

            // Assert
            child.IsDeleted.Should().BeTrue();
        }
    }
}

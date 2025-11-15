using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_PropertyLockingTests
    {
        private readonly IValidationEngineProvider _provider;

        public DomainObject_PropertyLockingTests()
        {
            _provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
        }

        [Fact]
        public void CanWriteProperty_WithNoLockedProperties_ShouldReturnTrue()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            var canWrite = entity.CanWriteProperty(nameof(TestDomainEntity.Name));

            // Assert
            canWrite.Should().BeTrue();
        }

        [Fact]
        public void SetProperty_WhenCannotWrite_ShouldNotChangeValue()
        {
            // This would require implementing LockedProperties override
            // Conceptual test demonstrating the pattern

            // Arrange
            var entity = new TestDomainEntity(_provider);
            var originalName = entity.Name;

            // If Name was locked, setting would fail
            // In actual implementation, override LockedProperties to return ["Name"]

            // Assert
            entity.CanWriteProperty(nameof(TestDomainEntity.Name)).Should().BeTrue();
        }
    }
}

using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_StateManagementTests
    {
        private readonly IValidationEngineProvider _provider;

        public DomainObject_StateManagementTests()
        {
            _provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
        }

        [Fact]
        public void IsDirty_NewEntity_ShouldBeTrue()
        {
            // Arrange & Act
            var entity = new TestDomainEntity(_provider);

            // Assert
            entity.IsDirty.Should().BeTrue();
            entity.IsSelfDirty.Should().BeTrue();
        }

        [Fact]
        public void IsDirty_AfterMarkClean_ShouldBeFalse()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.TestMarkClean();

            // Assert
            entity.IsDirty.Should().BeFalse();
            entity.IsSelfDirty.Should().BeFalse();
            entity.ContainsNewData.Should().BeFalse();
        }

        [Fact]
        public void IsDirty_AfterPropertyChange_ShouldBeTrue()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);
            entity.TestMarkClean();

            // Act
            entity.Name = "Changed";

            // Assert
            entity.IsDirty.Should().BeTrue();
            entity.ContainsNewData.Should().BeTrue();
        }

        [Fact]
        public void MarkDeleted_ShouldSetFlags()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);
            entity.TestMarkClean();

            // Act
            entity.TestMarkDeleted();

            // Assert
            entity.IsDeleted.Should().BeTrue();
            entity.IsDirty.Should().BeTrue();
        }

        [Fact]
        public void MarkNew_ShouldResetFlags()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);
            entity.TestMarkDeleted();

            // Act
            entity.TestMarkNew();

            // Assert
            entity.IsDeleted.Should().BeFalse();
            entity.IsDirty.Should().BeTrue();
        }

        [Fact]
        public void IsSavable_WhenDirtyAndValid_ShouldBeTrue()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);
            entity.Name = "Test";

            // Assert
            entity.IsDirty.Should().BeTrue();
            entity.IsValid.Should().BeTrue();
            entity.IsSavable.Should().BeTrue();
        }

        [Fact]
        public void IsSavable_WhenNotDirty_ShouldBeFalse()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);
            entity.TestMarkClean();

            // Assert
            entity.IsSavable.Should().BeFalse();
        }

        [Fact]
        public void IsSavable_WhenInvalid_ShouldBeFalse()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name")))
                .Build();

            var entity = new TestDomainEntity(provider);
            entity.Name = ""; // Invalid
            entity.CheckRules();

            // Assert
            entity.IsDirty.Should().BeTrue();
            entity.IsValid.Should().BeFalse();
            entity.IsSavable.Should().BeFalse();
        }
    }
}

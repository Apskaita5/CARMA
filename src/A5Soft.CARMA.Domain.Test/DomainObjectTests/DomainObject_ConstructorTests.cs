using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_ConstructorTests
    {
        [Fact]
        public void Constructor_WithValidationProvider_ShouldInitialize()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));

            // Act
            var entity = new TestDomainEntity(provider);

            // Assert
            entity.Should().NotBeNull();
            entity.IsDirty.Should().BeTrue("new entities are dirty");
            entity.IsValid.Should().BeTrue("no validation errors");
        }

        [Fact]
        public void Constructor_WithNullProvider_ShouldThrow()
        {
            IValidationEngineProvider nullProvider = null;
            // Act
            Action act = () => new TestDomainEntity(nullProvider);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("validationEngineProvider");
        }

        [Fact]
        public void CopyConstructor_ShouldCopyState()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var original = new TestDomainEntity(provider)
            {
                Name = "John",
                Age = 30
            };

            // Act
            var copy = new TestDomainEntity(original);

            // Assert
            copy.Should().NotBeSameAs(original);
            copy.Name.Should().Be("John");
            copy.Age.Should().Be(30);
        }

        [Fact]
        public void Constructor_ShouldCallInitialize()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));

            // Act
            var entity = new TestDomainEntity(provider);

            // Assert - entity should be initialized
            entity.Should().NotBeNull();
            entity.IsDirty.Should().BeTrue();
        }
    }
}

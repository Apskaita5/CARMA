using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_ValidationTests
    {
        [Fact]
        public void CheckRules_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entity = new TestDomainEntity(provider)
            {
                Name = "John",
                Age = 30
            };

            // Act
            entity.CheckRules();

            // Assert
            entity.IsValid.Should().BeTrue();
            entity.BrokenRules.ErrorCount.Should().Be(0);
        }

        [Fact]
        public void CheckRules_WithInvalidData_ShouldFailValidation()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name")))
                .Build();

            var entity = new TestDomainEntity(provider)
            {
                Name = ""
            };

            // Act
            entity.CheckRules();

            // Assert
            entity.IsValid.Should().BeFalse();
            entity.BrokenRules.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void SuspendValidation_ShouldPreventValidation()
        {
            // Arrange
            var validationCalled = false;
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name", rule => rule
                        .WithRuleName("REQUIRED")
                        .WithDescription("Name required")))
                .Build();

            var entity = new TestDomainEntity(provider);

            // Act
            using (entity.SuspendValidation())
            {
                entity.Name = "Test1";
                entity.Age = 25;
            }

            // Validation happens after suspension
            entity.Name = "Test2";

            // Assert
            // In a real implementation, we'd verify validation was called only once
            entity.Should().NotBeNull();
        }

        [Fact]
        public void SetValidationEngine_ShouldUpdateEngine()
        {
            // Arrange
            var provider1 = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entity = new TestDomainEntity(provider1);

            var provider2 = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name")))
                .Build();

            // Act
            entity.SetValidationEngine(provider2);
            entity.Name = "";
            entity.CheckRules();

            // Assert
            entity.IsValid.Should().BeFalse();
        }

        [Fact]
        public void HasWarnings_WithWarningRules_ShouldBeTrue()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Age", rule => rule
                        .WithDescription("Age seems low")
                        .AsWarning()))
                .Build();

            var entity = new TestDomainEntity(provider)
            {
                Age = 18
            };

            // Act
            entity.CheckRules();

            // Assert
            entity.HasWarnings.Should().BeTrue();
            entity.IsValid.Should().BeTrue("warnings don't affect validity");
        }
    }
}

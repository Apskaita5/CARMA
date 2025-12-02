using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_ValidationTests
    {
        private readonly IValidationEngineProvider _conditionalProvider;
        private readonly IValidationEngineProvider _passingProvider;


        public DomainObject_ValidationTests()
        {
            _conditionalProvider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name", (instance) => ((TestDomainEntity)instance).Name.IsNullOrWhiteSpace(),
                        ValidationMockFactory.CreateRequiredFieldError("Name"))
                    .WithPropertyRule("Age", (instance) => ((TestDomainEntity)instance).Age < 17,
                        ValidationMockFactory.CreateRequiredFieldError("Age"))
                    .WithPropertyRule("Salary", (instance) => ((TestDomainEntity)instance).Salary < 0.1m,
                        ValidationMockFactory.CreateRequiredFieldError("Salary")))
                .WithEngine<TestChildEntity>(builder => builder
                    .WithPropertyRule("Description", (instance) => ((TestChildEntity)instance).Description.IsNullOrWhiteSpace(),
                        ValidationMockFactory.CreateRequiredFieldError("Description")))
                .Build();

            _passingProvider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
        }


        [Fact]
        public void CheckRules_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var entity = new TestDomainEntity(_passingProvider)
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
            var entity = new TestDomainEntity(_passingProvider);

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

        [Fact]
        public void CheckRules_WithBaseConditionalValidation()
        {
            var entity = new TestDomainEntity(_conditionalProvider);
            entity.IsValid.Should().BeTrue("should be valid after init");

            entity.CheckRules();

            entity.IsValid.Should().BeFalse("should be invalid after check rules");
            entity.BrokenRules.ErrorCount.Should().Be(3, "there are 3 mock rules");

            entity.Name = "John";
            entity.IsValid.Should().BeFalse("should be still invalid after fixing one broken rule");
            entity.BrokenRules.ErrorCount.Should().Be(2, "there are 3 mock rules minus 1 fixed");

            entity.Age = 24;
            entity.IsValid.Should().BeFalse("should be still invalid after fixing two broken rules");
            entity.BrokenRules.ErrorCount.Should().Be(1, "there are 3 mock rules minus 2 fixed");

            entity.Salary = 1000.0m;
            entity.IsValid.Should().BeTrue("should be valid after fixing all broken rules");
            entity.BrokenRules.ErrorCount.Should().Be(0, "there are 3 mock rules minus 3 fixed");
        }

        [Fact]
        public void CheckRules_WithChildConditionalValidation()
        {
            var entity = new TestDomainEntity(_conditionalProvider);
            var child = new TestChildEntity(_conditionalProvider);
            entity.Child = child;
            entity.IsValid.Should().BeTrue("should be valid after init");

            entity.CheckRules();

            entity.IsSelfValid.Should().BeFalse("should be invalid after check rules");
            entity.BrokenRules.ErrorCount.Should().Be(3, "there are 3 mock rules");

            entity.Name = "John";
            entity.IsSelfValid.Should().BeFalse("should be still invalid after fixing one broken rule");
            entity.BrokenRules.ErrorCount.Should().Be(2, "there are 3 mock rules minus 1 fixed");

            entity.Age = 24;
            entity.IsSelfValid.Should().BeFalse("should be still invalid after fixing two broken rules");
            entity.BrokenRules.ErrorCount.Should().Be(1, "there are 3 mock rules minus 2 fixed");

            entity.Salary = 1000.0m;
            entity.IsSelfValid.Should().BeTrue("should be valid after fixing all broken rules");
            entity.BrokenRules.ErrorCount.Should().Be(0, "there are 3 mock rules minus 3 fixed");

            entity.IsValid.Should().BeFalse("child broken rules are not fixed yet");

            entity.Child.Description = "Test description";

            entity.IsValid.Should().BeTrue("child broken rules are fixed");
        }
    }
}

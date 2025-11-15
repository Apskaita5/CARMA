using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_RealWorldScenarios
    {
        [Fact]
        public void Scenario_UserRegistrationForm_Complete()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationRuleCollections.UserRegistration.NameRequired)
                    .WithPropertyRule("Age",
                        ValidationRuleCollections.UserRegistration.AgeMinimum))
                .Build();

            var user = new TestDomainEntity(provider)
            {
                NotifyPropertyChangedEnabled = true,
                BindingMode = BindingMode.Wpf
            };

            var validationMessages = new List<string>();

            // Act - Fill form
            user.Name = "John Doe";
            user.Age = 25;
            user.IsActive = true;

            user.CheckRules();

            // Assert
            user.IsValid.Should().BeTrue();
            user.IsSavable.Should().BeTrue();
        }

        [Fact]
        public void Scenario_EditExistingRecord_WithValidation()
        {
            // Arrange - Simulate loaded from database
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name")))
                .Build();

            var entity = new TestDomainEntity(provider)
            {
                Name = "Original Name",
                Age = 30
            };

            entity.TestMarkClean(); // Simulate saved state

            // Act - User edits
            entity.NotifyPropertyChangedEnabled = true;
            entity.Name = "Updated Name";

            // Assert
            entity.IsDirty.Should().BeTrue();
            entity.ContainsNewData.Should().BeTrue();
            entity.IsSavable.Should().BeTrue();
        }

        [Fact]
        public void Scenario_ParentChildForm_ComplexValidation()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name")))
                .WithEngine<TestChildEntity>(builder => builder
                    .WithPropertyRule("Description",
                        ValidationMockFactory.CreateRequiredFieldError("Description")))
                .Build();

            var parent = new TestDomainEntity(provider);
            var child = new TestChildEntity(provider);

            // Act - Setup relationship
            parent.Child = child;
            parent.Name = "Parent";
            child.Description = "Child";

            parent.CheckRules(checkRulesForChildren: true);

            // Assert
            parent.IsValid.Should().BeTrue();
            child.IsValid.Should().BeTrue();

            // Act - Make child invalid
            child.Description = "";
            parent.CheckRules(checkRulesForChildren: true);

            // Assert
            parent.IsSelfValid.Should().BeTrue();
            child.IsValid.Should().BeFalse();
            parent.IsValid.Should().BeFalse("child is invalid");
        }

        [Fact]
        public void Scenario_ConditionalFieldVisibility_BasedOnState()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entity = new TestDomainEntity(provider);

            // Act - Simulate conditional logic
            if (entity.IsActive)
            {
                // Show salary field
                entity.Salary = 50000m;
            }

            // Assert
            entity.Salary.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Scenario_WizardFlow_MultiStepValidation()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name"))
                    .WithPropertyRule("Age",
                        ValidationMockFactory.CreateRangeError("Age", 18, 120)))
                .Build();

            var entity = new TestDomainEntity(provider);

            // Act - Step 1: Basic info
            entity.Name = "John";
            entity.Age = 25;
            entity.CheckRules();

            var step1Valid = entity.IsValid;

            // Act - Step 2: Additional info
            entity.Salary = 50000m;
            entity.IsActive = true;
            entity.CheckRules();

            var step2Valid = entity.IsValid;

            // Assert
            step1Valid.Should().BeTrue();
            step2Valid.Should().BeTrue();
            entity.IsSavable.Should().BeTrue();
        }
    }
}

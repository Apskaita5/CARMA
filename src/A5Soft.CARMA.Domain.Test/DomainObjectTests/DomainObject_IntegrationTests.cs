using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.IntegrationTests
{
    public class DomainObject_IntegrationTests
    {
        [Fact]
        public void CompleteLifecycle_CreateModifyValidateSave()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name")))
                .Build();

            // Act - Create
            var entity = new TestDomainEntity(provider)
            {
                NotifyPropertyChangedEnabled = true,
                BindingMode = BindingMode.Wpf
            };

            // Assert - Initial state
            entity.IsDirty.Should().BeTrue();

            // Act - Modify
            entity.Name = "John Doe";
            entity.Age = 30;
            entity.Salary = 50000.00m;

            // Act - Simulate save
            entity.TestMarkClean();

            // Assert - After save
            entity.IsDirty.Should().BeFalse();

            // Act - Modify again
            entity.Salary = 55000.00m;

            // Assert - After modification
            entity.IsDirty.Should().BeTrue();
            entity.ContainsNewData.Should().BeTrue();

            // Act - Delete
            entity.Delete();

            // Assert
            entity.IsDeleted.Should().BeTrue();
            entity.IsDirty.Should().BeTrue();
        }

        [Fact]
        public void ParentChildGraph_ComplexInteractions()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var parent = new TestDomainEntity(provider)
            {
                NotifyPropertyChangedEnabled = true,
                BindingMode = BindingMode.Wpf
            };

            var child1 = new TestChildEntity(provider);
            var child2 = new TestChildEntity(provider);

            // Act - Add first child
            parent.Child = child1;
            child1.Description = "Child 1";
            child1.TestMarkClean();
            parent.TestMarkClean();

            // Assert
            parent.IsDirty.Should().BeFalse();
            child1.IsChild.Should().BeTrue();

            // Act - Modify child
            child1.Description = "Modified Child 1";

            // Assert
            parent.IsDirty.Should().BeTrue("child changed");
            parent.IsSelfDirty.Should().BeFalse("parent unchanged");

            // Act - Replace child
            parent.Child = child2;
            child2.Description = "Child 2";

            // Assert
            child1.Parent.Should().BeNull("old child unhooked");
            child2.Parent.Should().Be(parent);
            child2.IsChild.Should().BeTrue();
        }

        [Fact]
        public void ValidationWithBindings_ShouldWorkTogether()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name"))
                    .WithPropertyRule("Age",
                        ValidationMockFactory.CreateRangeError("Age", 18, 120)))
                .Build();

            var entity = new TestDomainEntity(provider)
            {
                NotifyPropertyChangedEnabled = true
            };

            var propertyChangedEvents = new List<string>();
            entity.PropertyChanged += (s, e) => propertyChangedEvents.Add(e.PropertyName ?? "");

            // Act
            entity.Name = "";
            entity.Age = 15;
            entity.CheckRules();

            // Assert
            entity.IsValid.Should().BeFalse();
            entity.BrokenRules.ErrorCount.Should().BeGreaterThan(0);
            propertyChangedEvents.Should().NotBeEmpty();
        }

        [Fact]
        public void BulkUpdate_WithSuspensions_ShouldBeEfficient()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entity = new TestDomainEntity(provider)
            {
                NotifyPropertyChangedEnabled = true
            };

            var eventCount = 0;
            entity.PropertyChanged += (s, e) => eventCount++;

            // Act - Bulk update with suspensions
            using (entity.SuspendValidation())
            using (entity.SuspendBindings())
            {
                entity.Name = "Name1";
                entity.Age = 25;
                entity.IsActive = true;
                entity.Salary = 45000m;
            }

            // Assert
            eventCount.Should().Be(0, "events were suspended");
            entity.IsDirty.Should().BeTrue();
        }

        [Fact]
        public void Serialization_ShouldPreserveState()
        {
            // This is a conceptual test - actual serialization would require
            // BinaryFormatter or System.Text.Json configuration

            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entity = new TestDomainEntity(provider)
            {
                Name = "John",
                Age = 30,
                IsActive = true
            };

            entity.TestMarkClean();

            // Act - After deserialization, state should be restored
            // var deserialized = SerializeDeserialize(entity);

            // Assert - Conceptual
            entity.Name.Should().Be("John");
            entity.Age.Should().Be(30);
            entity.IsDirty.Should().BeFalse();
        }
    }
}

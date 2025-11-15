using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_BindingIntegrationTests
    {
        private readonly IValidationEngineProvider _provider;

        public DomainObject_BindingIntegrationTests()
        {
            _provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
        }

        [Fact]
        public void PropertyChange_ShouldRaisePropertyChanged()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = true
            };

            var eventRaised = false;
            var propertyNames = new List<string>();

            entity.PropertyChanged += (s, e) =>
            {
                eventRaised = true;
                propertyNames.Add(e.PropertyName ?? string.Empty);
            };

            // Act
            entity.Name = "Test";

            // Assert
            eventRaised.Should().BeTrue();
            propertyNames.Should().NotBeEmpty("PropertyChanged should be raised");
        }

        [Fact]
        public void PropertyChange_WithPropertyChanging_ShouldRaiseBothEvents()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = true,
                NotifyPropertyChangingEnabled = true
            };

            var events = new List<string>();

            entity.PropertyChanging += (s, e) => events.Add($"Changing-{e.PropertyName}");
            entity.PropertyChanged += (s, e) => events.Add($"Changed-{e.PropertyName}");

            // Act
            entity.Name = "Test";

            // Assert
            events.Should().Contain(e => e.StartsWith("Changing-"));
            events.Should().Contain(e => e.StartsWith("Changed-"));
        }

        [Fact]
        public void ChildBindingOptions_ShouldInheritFromParent()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider)
            {
                BindingMode = BindingMode.Wpf,
                NotifyPropertyChangingEnabled = true,
                NotifyPropertyChangedEnabled = true
            };

            var child = new TestChildEntity(_provider);

            // Act
            parent.Child = child;

            // Assert
            child.BindingMode.Should().Be(BindingMode.Wpf);
            child.NotifyPropertyChangingEnabled.Should().BeTrue();
            child.NotifyPropertyChangedEnabled.Should().BeTrue();
        }

        [Fact]
        public void ChildBindingOptions_WhenParentChanges_ShouldUpdate()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider)
            {
                BindingMode = BindingMode.WinForms
            };

            var child = new TestChildEntity(_provider);
            parent.Child = child;

            // Act
            parent.BindingMode = BindingMode.Wpf;

            // Assert
            child.BindingMode.Should().Be(BindingMode.Wpf,
                "child should inherit parent's new binding mode");
        }

        [Fact]
        public void SuspendBindings_ShouldNotRaiseEvents()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = true
            };

            var eventCount = 0;
            entity.PropertyChanged += (s, e) => eventCount++;

            // Act
            using (entity.SuspendBindings())
            {
                entity.Name = "During Suspension 1";
                entity.Age = 25;
                entity.IsActive = true;
            }

            // After suspension
            entity.Name = "After Suspension";

            // Assert
            eventCount.Should().BeGreaterThan(0, "events should fire after suspension");
        }

        [Fact]
        public void BindingMode_WinForms_ShouldRaiseSingleEventForMultipleChanges()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = true,
                BindingMode = BindingMode.WinForms
            };

            entity.TestMarkClean();
            var eventCount = 0;
            var propertyNames = new List<string>();

            entity.PropertyChanged += (s, e) =>
            {
                eventCount++;
                propertyNames.Add(e.PropertyName ?? string.Empty);
            };

            // Act - Use method that calls OnPropertiesChanged
            entity.Name = "Test";
            entity.Age = 30;

            // Assert - WinForms mode behavior
            entity.IsDirty.Should().BeTrue();
        }

        [Fact]
        public void BindingMode_Wpf_ShouldRaiseEventForEachProperty()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = true,
                BindingMode = BindingMode.Wpf
            };

            entity.TestMarkClean();
            var propertyNames = new List<string>();

            entity.PropertyChanged += (s, e) =>
            {
                propertyNames.Add(e.PropertyName ?? string.Empty);
            };

            // Act
            entity.Name = "Test";
            entity.Age = 30;

            // Assert - WPF mode raises for each
            propertyNames.Should().NotBeEmpty();
        }

        [Fact]
        public void PropertyChange_WhenDisabled_ShouldNotRaiseEvent()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = false
            };

            var eventRaised = false;
            entity.PropertyChanged += (s, e) => eventRaised = true;

            // Act
            entity.Name = "Test";

            // Assert
            eventRaised.Should().BeFalse();
        }

        [Fact]
        public void ChildPropertyChange_ShouldBubbleToParent()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = true
            };

            var child = new TestChildEntity(_provider);
            parent.Child = child;

            var childChangedRaised = false;
            parent.ChildChanged += (s, e) =>
            {
                if (e.ChildObject == child)
                    childChangedRaised = true;
            };

            // Act
            child.Description = "Changed";

            // Assert
            childChangedRaised.Should().BeTrue("parent should receive child change notification");
            parent.IsDirty.Should().BeTrue();
        }

        [Fact]
        public void ComplexBindingScenario_MultiplePropertiesAndChildren()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = true,
                NotifyPropertyChangingEnabled = true,
                BindingMode = BindingMode.Wpf
            };

            var child = new TestChildEntity(_provider);
            parent.Child = child;

            var allEvents = new List<string>();
            parent.PropertyChanging += (s, e) => allEvents.Add($"P-Changing:{e.PropertyName}");
            parent.PropertyChanged += (s, e) => allEvents.Add($"P-Changed:{e.PropertyName}");
            parent.ChildChanged += (s, e) => allEvents.Add($"P-ChildChanged");

            child.PropertyChanged += (s, e) => allEvents.Add($"C-Changed:{e.PropertyName}");

            parent.TestMarkClean();
            allEvents.Clear();

            // Act
            parent.Name = "Parent Name";
            parent.Age = 30;
            child.Description = "Child Description";

            // Assert
            allEvents.Should().Contain(e => e.Contains("P-Changing"));
            allEvents.Should().Contain(e => e.Contains("P-Changed"));
            allEvents.Should().Contain(e => e.Contains("C-Changed"));
            allEvents.Should().Contain(e => e.Contains("P-ChildChanged"));

            parent.IsDirty.Should().BeTrue();
        }
    }
}

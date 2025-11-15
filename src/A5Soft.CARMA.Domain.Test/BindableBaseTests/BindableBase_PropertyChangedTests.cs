using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.BindableBaseTests
{
    public class BindableBase_PropertyChangedTests
    {
        [Fact]
        public void PropertyChanged_WhenEnabled_ShouldRaiseEvent()
        {
            // Arrange
            var entity = new TestBindableEntity
            {
                NotifyPropertyChangedEnabled = true
            };

            var eventRaised = false;
            string changedPropertyName = null;

            entity.PropertyChanged += (s, e) =>
            {
                eventRaised = true;
                changedPropertyName = e.PropertyName;
            };

            // Act
            entity.Name = "Test";

            // Assert
            eventRaised.Should().BeTrue();
            changedPropertyName.Should().Be(nameof(TestBindableEntity.Name));
        }

        [Fact]
        public void PropertyChanged_WhenDisabled_ShouldNotRaiseEvent()
        {
            // Arrange
            var entity = new TestBindableEntity
            {
                NotifyPropertyChangedEnabled = false
            };

            var eventRaised = false;
            entity.PropertyChanged += (s, e) => eventRaised = true;

            // Act
            entity.TestOnPropertyChanged(nameof(TestBindableEntity.Name));

            // Assert
            eventRaised.Should().BeFalse();
        }

        [Fact]
        public void PropertyChanged_MultipleProperties_WinFormsMode_ShouldRaiseOnce()
        {
            // Arrange
            var entity = new TestBindableEntity
            {
                NotifyPropertyChangedEnabled = true,
                BindingMode = BindingMode.WinForms
            };

            var eventCount = 0;
            var propertyNames = new List<string>();

            entity.PropertyChanged += (s, e) =>
            {
                eventCount++;
                propertyNames.Add(e.PropertyName ?? string.Empty);
            };

            // Act
            entity.TestOnPropertiesChanged(nameof(TestBindableEntity.Name),
                nameof(TestBindableEntity.Age));

            // Assert
            eventCount.Should().Be(1, "WinForms mode raises only first property");
            propertyNames[0].Should().Be(nameof(TestBindableEntity.Name));
        }

        [Fact]
        public void PropertyChanged_MultipleProperties_WpfMode_ShouldRaiseForEach()
        {
            // Arrange
            var entity = new TestBindableEntity
            {
                NotifyPropertyChangedEnabled = true,
                BindingMode = BindingMode.Wpf
            };

            var eventCount = 0;
            var propertyNames = new List<string>();

            entity.PropertyChanged += (s, e) =>
            {
                eventCount++;
                propertyNames.Add(e.PropertyName ?? string.Empty);
            };

            // Act
            entity.TestOnPropertiesChanged(nameof(TestBindableEntity.Name),
                nameof(TestBindableEntity.Age));

            // Assert
            eventCount.Should().Be(2, "WPF mode raises for each property");
            propertyNames.Should().Contain(nameof(TestBindableEntity.Name));
            propertyNames.Should().Contain(nameof(TestBindableEntity.Age));
        }

        [Fact]
        public void SuspendBindings_ShouldPreventEvents()
        {
            // Arrange
            var entity = new TestBindableEntity
            {
                NotifyPropertyChangedEnabled = true
            };

            var eventCount = 0;
            entity.PropertyChanged += (s, e) => eventCount++;

            // Act
            using (entity.SuspendBindings())
            {
                entity.TestOnPropertyChanged(nameof(TestBindableEntity.Name));
                entity.TestOnPropertyChanged(nameof(TestBindableEntity.Age));
            }

            // After suspension
            entity.TestOnPropertyChanged(nameof(TestBindableEntity.Name));

            // Assert
            eventCount.Should().Be(1, "events only after suspension ends");
        }
    }
}

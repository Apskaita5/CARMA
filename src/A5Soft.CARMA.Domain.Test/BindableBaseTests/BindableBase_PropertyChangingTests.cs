using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.BindableBaseTests
{
    public class BindableBase_PropertyChangingTests
    {
        [Fact]
        public void PropertyChanging_WhenEnabled_ShouldRaiseEvent()
        {
            // Arrange
            var entity = new TestBindableEntity
            {
                NotifyPropertyChangingEnabled = true
            };

            var eventRaised = false;
            string changingPropertyName = null;

            entity.PropertyChanging += (s, e) =>
            {
                eventRaised = true;
                changingPropertyName = e.PropertyName;
            };

            // Act
            entity.Name = "Test";

            // Assert
            eventRaised.Should().BeTrue();
            changingPropertyName.Should().Be(nameof(TestBindableEntity.Name));
        }

        [Fact]
        public void PropertyChanging_WhenDisabled_ShouldNotRaiseEvent()
        {
            // Arrange
            var entity = new TestBindableEntity
            {
                NotifyPropertyChangingEnabled = false
            };

            var eventRaised = false;
            entity.PropertyChanging += (s, e) => eventRaised = true;

            // Act
            entity.TestOnPropertyChanging(nameof(TestBindableEntity.Name));

            // Assert
            eventRaised.Should().BeFalse();
        }

        [Fact]
        public void PropertyChanging_ShouldFireBeforePropertyChanged()
        {
            // Arrange
            var entity = new TestBindableEntity
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
            events.Should().HaveCount(2);
            events[0].Should().StartWith("Changing-");
            events[1].Should().StartWith("Changed-");
        }
    }
}

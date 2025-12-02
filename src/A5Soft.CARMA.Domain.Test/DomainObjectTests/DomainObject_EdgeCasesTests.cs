using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_EdgeCasesTests
    {
        private readonly IValidationEngineProvider _provider;

        public DomainObject_EdgeCasesTests()
        {
            _provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
        }

        [Fact]
        public void SetProperty_WithNullString_ShouldConvertToEmpty()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.Name = null!;

            // Assert
            entity.Name.Should().Be(string.Empty);
        }

        [Fact]
        public void SetProperty_WithWhitespace_ShouldTrim()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.Name = "   Test   ";

            // Assert
            entity.Name.Should().Be("Test");
        }

        [Fact]
        public void CheckRules_WithNullPropertyNames_ShouldThrow()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            Action act = () => entity.TestCheckPropertyRules(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CheckRules_WithEmptyPropertyNames_ShouldThrow()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            Action act = () => entity.TestCheckPropertyRules();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetChildProperty_ToSameInstance_ShouldNotTriggerEvents()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider)
            {
                NotifyPropertyChangedEnabled = true
            };

            var child = new TestChildEntity(_provider);
            parent.Child = child;
            parent.TestMarkClean();

            var eventCount = 0;
            parent.PropertyChanged += (s, e) => eventCount++;

            // Act
            parent.Child = child; // Same instance

            // Assert
            eventCount.Should().Be(0);
            parent.IsSelfDirty.Should().BeFalse();
        }

        [Fact]
        public void DecimalProperty_WithMaxValue_ShouldHandle()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.Salary = decimal.MaxValue;

            // Assert
            entity.Salary.Should().Be(decimal.MaxValue);
        }

        [Fact]
        public void DateTimeProperty_WithMinMaxValues_ShouldHandle()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act & Assert - Min
            entity.BirthDate = DateTime.MinValue;
            entity.BirthDate.Date.Should().Be(DateTime.MinValue.Date);

            // Act & Assert - Max
            entity.BirthDate = DateTime.MaxValue;
            entity.BirthDate.Date.Should().Be(DateTime.MaxValue.Date);
        }

        [Fact]
        public void MultipleChildChanges_ShouldAccumulateInParent()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider);
            var child = new TestChildEntity(_provider);
            parent.Child = child;
            parent.TestMarkClean();

            // Act
            child.Description = "Change 1";
            child.Description = "Change 2";
            child.Description = "Change 3";

            // Assert
            parent.IsDirty.Should().BeTrue();
        }
    }
}

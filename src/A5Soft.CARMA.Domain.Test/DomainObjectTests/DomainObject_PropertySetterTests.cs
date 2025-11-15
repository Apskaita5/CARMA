using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_PropertySetterTests
    {
        private readonly IValidationEngineProvider _provider;

        public DomainObject_PropertySetterTests()
        {
            _provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
        }

        [Fact]
        public void SetPropertyValue_String_ShouldUpdateValue()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.Name = "John";

            // Assert
            entity.Name.Should().Be("John");
        }

        [Fact]
        public void SetPropertyValue_String_ShouldTrimByDefault()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.Name = "  John  ";

            // Assert
            entity.Name.Should().Be("John");
        }

        [Fact]
        public void SetPropertyValue_Int_ShouldUpdateValue()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.Age = 30;

            // Assert
            entity.Age.Should().Be(30);
        }

        [Fact]
        public void SetPropertyValue_Decimal_ShouldRoundToDigits()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.Salary = 1234.5678m;

            // Assert
            entity.Salary.Should().Be(1234.57m);
        }

        [Fact]
        public void SetPropertyValue_DateTime_ShouldIgnoreTimeByDefault()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);
            var date = new DateTime(2025, 1, 1, 10, 30, 0);

            // Act
            entity.BirthDate = date;

            // Assert
            entity.BirthDate.Should().Be(date.Date);
        }

        [Fact]
        public void SetPropertyValue_Bool_ShouldUpdateValue()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);

            // Act
            entity.IsActive = true;

            // Assert
            entity.IsActive.Should().BeTrue();
        }

        [Fact]
        public void SetPropertyValue_SameValue_ShouldNotMarkDirty()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);
            entity.Name = "John";
            entity.TestMarkClean();

            // Act
            entity.Name = "John";

            // Assert
            entity.IsDirty.Should().BeFalse();
        }

        [Fact]
        public void SetPropertyValue_DifferentValue_ShouldMarkDirty()
        {
            // Arrange
            var entity = new TestDomainEntity(_provider);
            entity.TestMarkClean();

            // Act
            entity.Name = "Jane";

            // Assert
            entity.IsDirty.Should().BeTrue();
        }
    }
}

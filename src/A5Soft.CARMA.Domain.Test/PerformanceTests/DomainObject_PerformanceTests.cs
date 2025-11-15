using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.PerformanceTests
{
    public class DomainObject_PerformanceTests
    {
        [Fact]
        public void CreateManyEntities_ShouldComplete()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entities = new List<TestDomainEntity>();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                entities.Add(new TestDomainEntity(provider));
            }

            // Assert
            entities.Should().HaveCount(1000);
        }

        [Fact]
        public void BulkPropertyChanges_ShouldComplete()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entity = new TestDomainEntity(provider);

            // Act
            var startTime = DateTime.UtcNow;

            for (int i = 0; i < 10000; i++)
            {
                entity.Name = $"Name{i}";
                entity.Age = i;
            }

            var duration = DateTime.UtcNow - startTime;

            // Assert
            duration.TotalMilliseconds.Should().BeLessThan(1000,
                "bulk updates should be fast");
        }
    }
}

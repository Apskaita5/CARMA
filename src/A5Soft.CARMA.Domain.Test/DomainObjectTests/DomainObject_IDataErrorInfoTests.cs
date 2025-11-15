using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using System.ComponentModel;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_IDataErrorInfoTests
    {
        [Fact]
        public void IDataErrorInfo_Error_WhenValid_ShouldBeEmpty()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entity = new TestDomainEntity(provider) as IDataErrorInfo;

            // Act
            var error = entity.Error;

            // Assert
            error.Should().BeEmpty();
        }

        [Fact]
        public void IDataErrorInfo_Error_WhenInvalid_ShouldReturnMessage()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name")))
                .Build();

            var entity = new TestDomainEntity(provider) as IDataErrorInfo;
            ((TestDomainEntity)entity).Name = "";
            ((TestDomainEntity)entity).CheckRules();

            // Act
            var error = entity.Error;

            // Assert
            error.Should().NotBeEmpty();
        }

        [Fact]
        public void IDataErrorInfo_Indexer_ForValidProperty_ShouldBeEmpty()
        {
            // Arrange
            var provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
            var entity = new TestDomainEntity(provider) as IDataErrorInfo;

            // Act
            var error = entity[nameof(TestDomainEntity.Name)];

            // Assert
            error.Should().BeEmpty();
        }

        [Fact]
        public void IDataErrorInfo_Indexer_ForInvalidProperty_ShouldReturnError()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name", "Full Name")))
                .Build();

            var entity = new TestDomainEntity(provider) as IDataErrorInfo;
            ((TestDomainEntity)entity).Name = "";
            ((TestDomainEntity)entity).CheckRules();

            // Act
            var error = entity[nameof(TestDomainEntity.Name)];

            // Assert
            error.Should().Contain("Full Name");
        }
    }
}

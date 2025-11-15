using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Matadata;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_MetadataTests
    {
        [Fact]
        public void GetMetadata_ShouldReturnEntityMetadata()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>()
                .Build();

            var entity = new TestDomainEntity(provider);

            // Act
            var result = entity.GetMetadata();

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void GetBrokenRulesTree_ShouldReturnTreeStructure()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestDomainEntity>(builder => builder
                    .WithPropertyRule("Name",
                        ValidationMockFactory.CreateRequiredFieldError("Name")))
                .Build();

            var entity = new TestDomainEntity(provider);
            entity.Name = "";
            entity.CheckRules();

            // Act
            var tree = entity.GetBrokenRulesTree(useInstanceDescription: false);

            // Assert
            tree.Should().NotBeNull();
            tree.Count(Rules.RuleSeverity.Error).Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetBrokenRulesTree_WithChildren_ShouldIncludeChildRules()
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
            parent.Child = child;

            parent.Name = "";
            child.Description = "";
            parent.CheckRules(checkRulesForChildren: true);

            // Act
            var tree = parent.GetBrokenRulesTree();

            // Assert
            tree.Should().NotBeNull();
            tree.Count(Rules.RuleSeverity.Error).Should().Be(2);
        }
    }
}

using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules;
using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectTests
{
    public class DomainObject_ChildManagementTests
    {
        private readonly IValidationEngineProvider _provider;

        public DomainObject_ChildManagementTests()
        {
            _provider = ValidationMockFactory.CreatePassingProvider(typeof(TestDomainEntity), typeof(TestChildEntity));
        }

        [Fact]
        public void SetChild_ShouldSetParentReference()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider);
            var child = new TestChildEntity(_provider);

            // Act
            parent.Child = child;

            // Assert
            child.Parent.Should().Be(parent);
            child.IsChild.Should().BeTrue();
        }

        [Fact]
        public void ChildPropertyChange_ShouldMarkParentDirty()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider);
            var child = new TestChildEntity(_provider);
            parent.Child = child;
            parent.TestMarkClean();

            // Act
            child.Description = "Changed";

            // Assert
            parent.IsDirty.Should().BeTrue();
            parent.IsSelfDirty.Should().BeFalse();
        }

        [Fact]
        public void ReplaceChild_ShouldUnhookOldChild()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider);
            var oldChild = new TestChildEntity(_provider);
            var newChild = new TestChildEntity(_provider);

            parent.Child = oldChild;
            parent.TestMarkClean();

            // Act
            parent.Child = newChild;

            // Assert
            oldChild.Parent.Should().BeNull();
            newChild.Parent.Should().Be(parent);
        }

        [Fact]
        public void SetChildToNull_ShouldUnhookChild()
        {
            // Arrange
            var parent = new TestDomainEntity(_provider);
            var child = new TestChildEntity(_provider);
            parent.Child = child;

            // Act
            parent.Child = null;

            // Assert
            child.Parent.Should().BeNull();
        }

        [Fact]
        public void ChildValidation_ShouldAffectParentValidity()
        {
            // Arrange
            var provider = new ValidationEngineProviderMockBuilder()
                .WithEngine<TestChildEntity>(builder => builder
                    .WithPropertyRule("Description",
                        ValidationMockFactory.CreateRequiredFieldError("Description")))
                .Build();

            var parent = new TestDomainEntity(_provider);
            var child = new TestChildEntity(provider)
            {
                Description = ""
            };

            parent.Child = child;

            // Act
            parent.CheckRules(checkRulesForChildren: true);

            // Assert
            parent.IsSelfValid.Should().BeTrue();
            child.IsValid.Should().BeFalse();
            parent.IsValid.Should().BeFalse("child is invalid");
        }
    }
}

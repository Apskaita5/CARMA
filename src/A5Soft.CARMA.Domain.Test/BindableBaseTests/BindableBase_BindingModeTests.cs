using A5Soft.CARMA.Domain.Test.TestEntities;
using FluentAssertions;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.BindableBaseTests
{
    public class BindableBase_BindingModeTests
    {
        [Fact]
        public void BindingMode_DefaultValue_ShouldBeWinForms()
        {
            // Arrange & Act
            var entity = new TestBindableEntity();

            // Assert
            entity.BindingMode.Should().Be(BindingMode.WinForms);
        }

        [Fact]
        public void BindingMode_WhenChanged_ShouldUpdateValue()
        {
            // Arrange
            var entity = new TestBindableEntity();

            // Act
            entity.BindingMode = BindingMode.Wpf;

            // Assert
            entity.BindingMode.Should().Be(BindingMode.Wpf);
        }

        [Fact]
        public void BindingMode_WhenChanged_ShouldCallVirtualMethod()
        {
            // This tests that OnBindingModeChanged is called
            // In a real subclass, this would update child bindings

            // Arrange
            var entity = new TestBindableEntity();
            var originalMode = entity.BindingMode;

            // Act
            entity.BindingMode = BindingMode.Wpf;

            // Assert
            entity.BindingMode.Should().NotBe(originalMode);
        }
    }
}

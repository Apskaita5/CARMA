using A5Soft.CARMA.Domain.Reflection;
using A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.ReflectionTests
{
    public class ReflectionTests
    {
        #region GetCustomAttributesWithInheritance<TAttribute>(Type) Tests

        [Fact]
        public void GetCustomAttributesWithInheritance_Type_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                type.GetCustomAttributesWithInheritance<TestAttribute>());
        }

        [Fact]
        public void GetCustomAttributesWithInheritance_Type_NoAttributes_ReturnsEmptyList()
        {
            // Arrange
            var type = typeof(NoAttributeClass);

            // Act
            var result = type.GetCustomAttributesWithInheritance<TestAttribute>();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetCustomAttributesWithInheritance_Type_WithAttribute_ReturnsAttribute()
        {
            // Arrange
            var type = typeof(BaseClass);

            // Act
            var result = type.GetCustomAttributesWithInheritance<TestAttribute>();

            // Assert
            Assert.Single(result);
            Assert.Equal("Base", result[0].Value);
        }

        [Fact]
        public void GetCustomAttributesWithInheritance_Type_WithInheritance_ReturnsAllAttributes()
        {
            // Arrange
            var type = typeof(DerivedClass);

            // Act
            var result = type.GetCustomAttributesWithInheritance<TestAttribute>();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Value == "Derived");
            Assert.Contains(result, a => a.Value == "Base");
        }

        [Fact]
        public void GetCustomAttributesWithInheritance_Type_WithInterfaces_ReturnsInterfaceAttributes()
        {
            // Arrange
            var type = typeof(ImplementationClass);

            // Act
            var result = type.GetCustomAttributesWithInheritance<TestAttribute>();

            // Assert
            Assert.True(result.Count >= 3);
            Assert.Contains(result, a => a.Value == "Implementation");
            Assert.Contains(result, a => a.Value == "Base");
            Assert.Contains(result, a => a.Value == "Interface1");
        }

        #endregion

        #region GetCustomAttributesWithInheritance<TAttribute>(PropertyInfo) Tests

        [Fact]
        public void GetCustomAttributesWithInheritance_Property_NullProperty_ThrowsArgumentNullException()
        {
            // Arrange
            PropertyInfo prop = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                prop.GetCustomAttributesWithInheritance<TestAttribute>());
        }

        [Fact]
        public void GetCustomAttributesWithInheritance_Property_NoAttributes_ReturnsEmptyList()
        {
            // Arrange
            var prop = typeof(NoAttributeClass).GetProperty("Property1");

            // Act
            var result = prop.GetCustomAttributesWithInheritance<TestAttribute>();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetCustomAttributesWithInheritance_Property_WithAttribute_ReturnsAttribute()
        {
            // Arrange
            var prop = typeof(BaseClass).GetProperty("Property1");

            // Act
            var result = prop.GetCustomAttributesWithInheritance<TestAttribute>();

            // Assert
            Assert.Single(result);
            Assert.Equal("BaseProp", result[0].Value);
        }

        [Fact]
        public void GetCustomAttributesWithInheritance_Property_WithInheritedProperty_ReturnsAllAttributes()
        {
            // Arrange
            var prop = typeof(DerivedClass).GetProperty("Property1");

            // Act
            var result = prop.GetCustomAttributesWithInheritance<TestAttribute>();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Value == "DerivedProp");
            Assert.Contains(result, a => a.Value == "BaseProp");
        }

        [Fact]
        public void GetCustomAttributesWithInheritance_Property_MultipleAttributes_ReturnsAll()
        {
            // Arrange
            var prop = typeof(ClassWithMultipleAttributes).GetProperty("MultiAttributeProperty");

            // Act
            var result = prop.GetCustomAttributesWithInheritance<TestAttribute>();

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetCustomAttributeWithInheritance<TAttribute>(PropertyInfo) Tests

        [Fact]
        public void GetCustomAttributeWithInheritance_NullProperty_ThrowsArgumentNullException()
        {
            // Arrange
            PropertyInfo prop = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                prop.GetCustomAttributeWithInheritance<TestAttribute>());
        }

        [Fact]
        public void GetCustomAttributeWithInheritance_NoAttribute_ReturnsNull()
        {
            // Arrange
            var prop = typeof(NoAttributeClass).GetProperty("Property1");

            // Act
            var result = prop.GetCustomAttributeWithInheritance<TestAttribute>();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCustomAttributeWithInheritance_WithAttribute_ReturnsFirstAttribute()
        {
            // Arrange
            var prop = typeof(BaseClass).GetProperty("Property1");

            // Act
            var result = prop.GetCustomAttributeWithInheritance<TestAttribute>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("BaseProp", result.Value);
        }

        [Fact]
        public void GetCustomAttributeWithInheritance_WithInheritance_ReturnsFirstAttribute()
        {
            // Arrange
            var prop = typeof(DerivedClass).GetProperty("Property1");

            // Act
            var result = prop.GetCustomAttributeWithInheritance<TestAttribute>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("DerivedProp", result.Value);
        }

        #endregion

        #region GetPublicProperties Tests

        [Fact]
        public void GetPublicProperties_Generic_ReturnsClassProperties()
        {
            // Act
            var result = Reflection.Extensions.GetPublicProperties<BaseClass>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Contains(result, p => p.Name == "Property1");
            Assert.Contains(result, p => p.Name == "Property2");
        }

        [Fact]
        public void GetPublicProperties_GenericWithFilter_ReturnsFilteredProperties()
        {
            // Act
            var result = Reflection.Extensions.GetPublicProperties<BaseClass>(p => p.Name == "Property1");

            // Assert
            Assert.Single(result);
            Assert.Equal("Property1", result[0].Name);
        }

        [Fact]
        public void GetPublicProperties_Type_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => type.GetPublicProperties());
        }

        [Fact]
        public void GetPublicProperties_Class_ReturnsProperties()
        {
            // Arrange
            var type = typeof(DerivedClass);

            // Act
            var result = type.GetPublicProperties();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 3);
        }

        [Fact]
        public void GetPublicProperties_Interface_ReturnsAllInterfaceProperties()
        {
            // Arrange
            var type = typeof(ITestInterface2);

            // Act
            var result = type.GetPublicProperties();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(result, p => p.Name == "Property1");
            Assert.Contains(result, p => p.Name == "Property2");
        }

        [Fact]
        public void GetPublicProperties_EmptyInterface_ReturnsEmptyArray()
        {
            // Arrange
            var type = typeof(IEmptyInterface);

            // Act
            var result = type.GetPublicProperties();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetPublicProperties_Struct_ThrowsInvalidOperationException()
        {
            // Arrange
            var type = typeof(int);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => type.GetPublicProperties());
        }

        [Fact]
        public void GetPublicProperties_WithFilter_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                type.GetPublicProperties(p => true));
        }

        [Fact]
        public void GetPublicProperties_WithFilter_NullFilter_ThrowsArgumentNullException()
        {
            // Arrange
            var type = typeof(BaseClass);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                type.GetPublicProperties(null));
        }

        [Fact]
        public void GetPublicProperties_ClassWithFilter_ReturnsFilteredProperties()
        {
            // Arrange
            var type = typeof(DerivedClass);

            // Act
            var result = type.GetPublicProperties(p => p.Name.Contains("1"));

            // Assert
            Assert.Single(result);
            Assert.Equal("Property1", result[0].Name);
        }

        [Fact]
        public void GetPublicProperties_InterfaceWithFilter_ReturnsFilteredProperties()
        {
            // Arrange
            var type = typeof(ITestInterface2);

            // Act
            var result = type.GetPublicProperties(p => p.Name == "Property1");

            // Assert
            Assert.Single(result);
            Assert.Equal("Property1", result[0].Name);
        }

        [Fact]
        public void GetPublicProperties_InterfaceWithFilter_NoMatches_ReturnsEmpty()
        {
            // Arrange
            var type = typeof(ITestInterface2);

            // Act
            var result = type.GetPublicProperties(p => p.Name == "NonExistent");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion
    }
}

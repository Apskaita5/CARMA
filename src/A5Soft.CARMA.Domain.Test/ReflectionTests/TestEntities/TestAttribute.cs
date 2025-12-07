using System;

namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Interface, AllowMultiple = true)]
    class TestAttribute : Attribute
    {
        public string Value { get; set; }
        public TestAttribute(string value) => Value = value;
    }
}

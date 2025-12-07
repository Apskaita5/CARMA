using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    class ClassWithMultipleAttributes
    {
        [Test("First")]
        [Test("Second")]
        [Required]
        public string MultiAttributeProperty { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    // Test enums
    public enum SimpleEnum
    {
        [Display(Name = "First Value", ShortName = "First", Description = "This is the first value")]
        Value1,

        [Display(Name = "Second Value", ShortName = "Second", Description = "This is the second value")]
        Value2,

        Value3 // No display attribute
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    [Flags]
    public enum FlagsEnum
    {
        [Display(Name = "No Permissions", ShortName = "None", Description = "User has no permissions")]
        None = 0,

        [Display(Name = "Can Read", ShortName = "Read", Description = "User can read data")]
        Read = 1,

        [Display(Name = "Can Write", ShortName = "Write", Description = "User can write data")]
        Write = 2,

        [Display(Name = "Can Delete", ShortName = "Delete", Description = "User can delete data")]
        Delete = 4,

        Execute = 8 // No display attribute
    }
}

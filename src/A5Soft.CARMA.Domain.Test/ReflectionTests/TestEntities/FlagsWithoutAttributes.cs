using System;

namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    [Flags]
    public enum FlagsWithoutAttributes
    {
        None = 0,
        Flag1 = 1,
        Flag2 = 2,
        Flag3 = 4
    }
}

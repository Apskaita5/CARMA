using A5Soft.CARMA.Domain.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace A5Soft.CARMA.Domain.Test
{
    public class LookupTest
    {
        [Fact]
        public void TestLookupEqualities()
        {
            var firstIdString = "s6f4f65sd46fs";
            var secondIdString = "6dfg8d4g684f8sdg";
            DomainEntityIdentity<SimpleDomainEntity> nullId = null;
            DomainEntityIdentity<SimpleDomainEntity> firstId = firstIdString;
            DomainEntityIdentity<SimpleDomainEntity> secondId = secondIdString;
            MockSimpleLookup nullLookup = null;
            var firstLookup = new MockSimpleLookup(firstIdString);
            var secondLookup = new MockSimpleLookup(secondIdString);

            var isEqual = nullId == nullLookup;
            Assert.True(isEqual, "Null lookup NOT equals null identity.");
            isEqual = nullId == firstLookup;
            Assert.False(isEqual, "Non null lookup equals null identity.");
            isEqual = firstId == firstLookup;
            Assert.True(isEqual, "Lookup NOT equals identity with the same key.");
            isEqual = firstId == secondLookup;
            Assert.False(isEqual, "Lookup equals identity with different key.");

        }
    }
}

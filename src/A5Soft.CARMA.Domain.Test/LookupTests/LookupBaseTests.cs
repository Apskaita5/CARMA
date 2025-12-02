using A5Soft.CARMA.Domain.Test.TestEntities;
using System.Collections.Generic;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.LookupTests
{
    public class LookupBaseTests
    {
        [Fact]
        public void Id_ReturnsIdentity()
        {
            var lookup = new TestLookup("id1", "Test");

            Assert.NotNull(lookup.Id);
            Assert.Equal("id1", lookup.Id.Key);
        }

        [Fact]
        public void Equals_WithSameId_ReturnsTrue()
        {
            var lookup1 = new TestLookup("id1", "Name1");
            var lookup2 = new TestLookup("ID1", "Name2");

            Assert.True(lookup1.Equals(lookup2));
            Assert.True(lookup1 == lookup2);
            Assert.False(lookup1 != lookup2);
        }

        [Fact]
        public void Equals_WithDifferentId_ReturnsFalse()
        {
            var lookup1 = new TestLookup("id1", "Name");
            var lookup2 = new TestLookup("id2", "Name");

            Assert.False(lookup1.Equals(lookup2));
            Assert.False(lookup1 == lookup2);
            Assert.True(lookup1 != lookup2);
        }

        [Fact]
        public void Equals_WithNull_ReturnsFalse()
        {
            var lookup = new TestLookup("id1", "Name");

            Assert.False(lookup.Equals(null));
            Assert.False(lookup == null);
            Assert.True(lookup != null);
        }

        [Fact]
        public void Equals_BothNull_ReturnsTrue()
        {
            TestLookup lookup1 = null;
            TestLookup lookup2 = null;

            Assert.True(lookup1 == lookup2);
            Assert.False(lookup1 != lookup2);
        }

        [Fact]
        public void GetHashCode_WithSameId_ReturnsSameHashCode()
        {
            var lookup1 = new TestLookup("id1", "Name1");
            var lookup2 = new TestLookup("ID1", "Name2");

            Assert.Equal(lookup1.GetHashCode(), lookup2.GetHashCode());
        }

        [Fact]
        public void ImplicitConversion_ToDomainEntityIdentity_ReturnsId()
        {
            var lookup = new TestLookup("id1", "Name");
            DomainEntityIdentity<TestDomainEntity> id = lookup;

            Assert.NotNull(id);
            Assert.Equal("id1", id.Key);
        }

        [Fact]
        public void ImplicitConversion_NullLookup_ReturnsNull()
        {
            TestLookup lookup = null;
            DomainEntityIdentity<TestDomainEntity> id = lookup;

            Assert.Null(id);
        }

        [Fact]
        public void Match_WithMatchingString_ReturnsTrue()
        {
            var lookup = new TestLookup("id1", "TestName");

            Assert.True(lookup.Match("Test"));
            Assert.True(lookup.Match("NAME"));
            Assert.True(lookup.Match("testname"));
        }

        [Fact]
        public void Match_WithNonMatchingString_ReturnsFalse()
        {
            var lookup = new TestLookup("id1", "TestName");

            Assert.False(lookup.Match("Other"));
        }

        [Fact]
        public void WorksWithHashSet()
        {
            var set = new HashSet<TestLookup>
            {
                new TestLookup("id1", "Name1"),
                new TestLookup("ID1", "Name2"), // Duplicate
                new TestLookup("id2", "Name3")
            };

            Assert.Equal(2, set.Count);
        }

        [Fact]
        public void WorksWithDictionary()
        {
            var dict = new Dictionary<TestLookup, string>
            {
                [new TestLookup("key1", "Name1")] = "value1",
                [new TestLookup("KEY1", "Name2")] = "value2" // Overwrites
            };

            Assert.Single(dict);
        }
    }
}

using A5Soft.CARMA.Domain.Test.TestEntities;
using System;
using System.Collections.Generic;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.DomainObjectIdentityTests
{
    public class DomainEntityIdentityTests
    {
        [Fact]
        public void Constructor_WithValidKey_CreatesInstance()
        {
            var identity = new DomainEntityIdentity<TestDomainEntity>("ABC123");

            Assert.NotNull(identity);
            Assert.Equal("abc123", identity.Key);
        }

        [Fact]
        public void Constructor_NormalizesKey_ToLowerInvariant()
        {
            var identity = new DomainEntityIdentity<TestDomainEntity>("  MixedCase123  ");

            Assert.Equal("mixedcase123", identity.Key);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidKey_ThrowsArgumentNullException(string key)
        {
            Assert.Throws<ArgumentNullException>(() => new DomainEntityIdentity<TestDomainEntity>(key));
        }

        [Fact]
        public void Equals_WithSameKey_ReturnsTrue()
        {
            var id1 = new DomainEntityIdentity<TestDomainEntity>("test");
            var id2 = new DomainEntityIdentity<TestDomainEntity>("TEST");

            Assert.True(id1.Equals(id2));
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Fact]
        public void Equals_WithDifferentKey_ReturnsFalse()
        {
            var id1 = new DomainEntityIdentity<TestDomainEntity>("test1");
            var id2 = new DomainEntityIdentity<TestDomainEntity>("test2");

            Assert.False(id1.Equals(id2));
            Assert.False(id1 == id2);
            Assert.True(id1 != id2);
        }

        [Fact]
        public void Equals_WithNull_ReturnsFalse()
        {
            var id = new DomainEntityIdentity<TestDomainEntity>("test");

            Assert.False(id.Equals(null));
            Assert.False(id == null);
            Assert.True(id != null);
        }

        [Fact]
        public void Equals_BothNull_ReturnsTrue()
        {
            DomainEntityIdentity<TestDomainEntity> id1 = null;
            DomainEntityIdentity<TestDomainEntity> id2 = null;

            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Fact]
        public void GetHashCode_WithSameKey_ReturnsSameHashCode()
        {
            var id1 = new DomainEntityIdentity<TestDomainEntity>("test");
            var id2 = new DomainEntityIdentity<TestDomainEntity>("TEST");

            Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_IsCached_ReturnsConsistentValue()
        {
            var id = new DomainEntityIdentity<TestDomainEntity>("test");
            var hash1 = id.GetHashCode();
            var hash2 = id.GetHashCode();

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void ImplicitConversion_ToString_ReturnsKey()
        {
            var id = new DomainEntityIdentity<TestDomainEntity>("test");
            string key = id;

            Assert.Equal("test", key);
        }

        [Fact]
        public void ImplicitConversion_FromString_CreatesIdentity()
        {
            DomainEntityIdentity<TestDomainEntity> id = "test";

            Assert.NotNull(id);
            Assert.Equal("test", id.Key);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ImplicitConversion_FromInvalidString_ReturnsNull(string value)
        {
            DomainEntityIdentity<TestDomainEntity> id = value;

            Assert.Null(id);
        }

        [Fact]
        public void ToString_ReturnsKey()
        {
            var id = new DomainEntityIdentity<TestDomainEntity>("test123");

            Assert.Equal("test123", id.ToString());
        }

        [Fact]
        public void WorksWithHashSet()
        {
            var set = new HashSet<DomainEntityIdentity<TestDomainEntity>>
            {
                new DomainEntityIdentity<TestDomainEntity>("id1"),
                new DomainEntityIdentity<TestDomainEntity>("ID1"), // Duplicate
                new DomainEntityIdentity<TestDomainEntity>("id2")
            };

            Assert.Equal(2, set.Count);
        }

        [Fact]
        public void WorksWithDictionary()
        {
            var dict = new Dictionary<DomainEntityIdentity<TestDomainEntity>, string>
            {
                [new DomainEntityIdentity<TestDomainEntity>("key1")] = "value1",
                [new DomainEntityIdentity<TestDomainEntity>("KEY1")] = "value2" // Overwrites
            };

            Assert.Single(dict);
            Assert.Equal("value2", dict[new DomainEntityIdentity<TestDomainEntity>("key1")]);
        }
    }
}

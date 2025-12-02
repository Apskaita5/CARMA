using A5Soft.CARMA.Domain.Test.TestEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.LookupTests
{
    public class LookupCacheFactoryTests
    {
        [Fact]
        public void Create_WithValidLookup_CreatesCache()
        {
            var lookups = new[]
            {
                new TestLookup("id1", "Name1"),
                new TestLookup("id2", "Name2")
            };

            var cache = LookupCacheFactory.Create(lookups);

            Assert.NotNull(cache);
            Assert.Equal(2, cache.Count);
        }

        [Fact]
        public void Create_ReturnsCacheWithCorrectType()
        {
            var lookups = new[] { new TestLookup("id1", "Name1") };

            var cache = LookupCacheFactory.Create(lookups);

            Assert.IsAssignableFrom<ILookupCache<TestLookup>>(cache);
        }

        [Fact]
        public void Create_WithDifferentEntityTypes_CreatesCorrectCaches()
        {
            var testLookups = new[] { new TestLookup("id1", "Name1") };
            var otherLookups = new[] { new OtherLookup("id2", "Description1") };

            var testCache = LookupCacheFactory.Create(testLookups);
            var otherCache = LookupCacheFactory.Create(otherLookups);

            Assert.NotNull(testCache);
            Assert.NotNull(otherCache);
            Assert.IsType<TestLookup>(testCache.GetAll().First());
            Assert.IsType<OtherLookup>(otherCache.GetAll().First());
        }

        [Fact]
        public void GetCacheType_ReturnsCorrectType()
        {
            var cacheType = LookupCacheFactory.GetCacheType<TestLookup>();

            Assert.NotNull(cacheType);
            Assert.True(cacheType.IsGenericType);
            Assert.Equal(typeof(LookupCache<,>), cacheType.GetGenericTypeDefinition());
        }

        [Fact]
        public void GetCacheType_CachesResult()
        {
            var type1 = LookupCacheFactory.GetCacheType<TestLookup>();
            var type2 = LookupCacheFactory.GetCacheType<TestLookup>();

            Assert.Same(type1, type2);
        }

        [Fact]
        public void GetCacheType_WithDifferentLookups_ReturnsDifferentTypes()
        {
            var testType = LookupCacheFactory.GetCacheType<TestLookup>();
            var otherType = LookupCacheFactory.GetCacheType<OtherLookup>();

            Assert.NotEqual(testType, otherType);
        }

        //[Fact]
        //public void Create_WorksWithInheritedLookups()
        //{
        //    var lookups = new[] { new TestLookup("id1", "Name1") };
        //    var cache = LookupCacheFactory.Create(lookups);

        //    var result = cache.FindById(new DomainEntityIdentity<TestDomainEntity>("id1"));
        //    Assert.NotNull(result);
        //}
    }
}

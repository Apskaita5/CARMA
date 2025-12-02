using A5Soft.CARMA.Domain.Test.TestEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.LookupTests
{
    public class LookupCacheTests
    {
        [Fact]
        public void Constructor_WithValidLookups_CreatesCache()
        {
            var lookups = new[]
            {
                new TestLookup("id1", "Name1"),
                new TestLookup("id2", "Name2")
            };

            var cache = new LookupCache<TestDomainEntity, TestLookup>(lookups);

            Assert.Equal(2, cache.Count);
        }

        [Fact]
        public void Constructor_WithNullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LookupCache<TestDomainEntity, TestLookup>(null));
        }

        [Fact]
        public void Constructor_WithNullId_ThrowsInvalidOperationException()
        {
            var lookup = new TestLookup("id1", "Name");
            lookup.GetType().GetField("_id",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(lookup, null);

            Assert.Throws<InvalidOperationException>(() =>
                new LookupCache<TestDomainEntity, TestLookup>(new[] { lookup }));
        }

        [Fact]
        public void GetAll_ReturnsAllLookups()
        {
            var lookups = new[]
            {
                new TestLookup("id1", "Name1"),
                new TestLookup("id2", "Name2"),
                new TestLookup("id3", "Name3")
            };
            var cache = new LookupCache<TestDomainEntity, TestLookup>(lookups);

            var result = cache.GetAll();

            Assert.Equal(3, result.Count);
            Assert.Contains(result, l => l.Id.Key == "id1");
            Assert.Contains(result, l => l.Id.Key == "id2");
            Assert.Contains(result, l => l.Id.Key == "id3");
        }

        [Fact]
        public void GetAll_ReturnsReadOnlyCollection()
        {
            var lookups = new[] { new TestLookup("id1", "Name1") };
            var cache = new LookupCache<TestDomainEntity, TestLookup>(lookups);

            var result = cache.GetAll();

            Assert.IsAssignableFrom<IReadOnlyCollection<TestLookup>>(result);
        }

        [Fact]
        public void FindById_WithExistingId_ReturnsLookup()
        {
            var lookup1 = new TestLookup("id1", "Name1");
            var lookup2 = new TestLookup("id2", "Name2");
            var cache = new LookupCache<TestDomainEntity, TestLookup>(new[] { lookup1, lookup2 });

            var result = cache.FindById(new DomainEntityIdentity<TestDomainEntity>("ID1"));

            Assert.NotNull(result);
            Assert.Equal("id1", result.Id.Key);
            Assert.Equal("Name1", result.Name);
        }

        [Fact]
        public void FindById_WithNonExistingId_ReturnsNull()
        {
            var cache = new LookupCache<TestDomainEntity, TestLookup>(
                new[] { new TestLookup("id1", "Name1") });

            var result = cache.FindById(new DomainEntityIdentity<TestDomainEntity>("id999"));

            Assert.Null(result);
        }

        [Fact]
        public void FindByKey_WithExistingKey_ReturnsLookup()
        {
            var lookup = new TestLookup("id1", "Name1");
            var cache = new LookupCache<TestDomainEntity, TestLookup>(new[] { lookup });

            var result = cache.FindByKey("ID1");

            Assert.NotNull(result);
            Assert.Equal("id1", result.Id.Key);
        }

        [Fact]
        public void Contains_WithExistingId_ReturnsTrue()
        {
            var cache = new LookupCache<TestDomainEntity, TestLookup>(
                new[] { new TestLookup("id1", "Name1") });

            Assert.True(cache.Contains(new DomainEntityIdentity<TestDomainEntity>("ID1")));
        }

        [Fact]
        public void Contains_WithNonExistingId_ReturnsFalse()
        {
            var cache = new LookupCache<TestDomainEntity, TestLookup>(
                new[] { new TestLookup("id1", "Name1") });

            Assert.False(cache.Contains(new DomainEntityIdentity<TestDomainEntity>("id999")));
        }

        [Fact]
        public void TryGetById_WithExistingId_ReturnsTrueAndLookup()
        {
            var lookup = new TestLookup("id1", "Name1");
            var cache = new LookupCache<TestDomainEntity, TestLookup>(new[] { lookup });

            var result = cache.TryGetById(
                new DomainEntityIdentity<TestDomainEntity>("ID1"), out var foundLookup);

            Assert.True(result);
            Assert.NotNull(foundLookup);
            Assert.Equal("id1", foundLookup.Id.Key);
        }

        [Fact]
        public void TryGetById_WithNonExistingId_ReturnsFalse()
        {
            var cache = new LookupCache<TestDomainEntity, TestLookup>(
                new[] { new TestLookup("id1", "Name1") });

            var result = cache.TryGetById(
                new DomainEntityIdentity<TestDomainEntity>("id999"), out var foundLookup);

            Assert.False(result);
            Assert.Null(foundLookup);
        }

        [Fact]
        public void Count_ReturnsCorrectCount()
        {
            var lookups = new[]
            {
                new TestLookup("id1", "Name1"),
                new TestLookup("id2", "Name2"),
                new TestLookup("id3", "Name3")
            };
            var cache = new LookupCache<TestDomainEntity, TestLookup>(lookups);

            Assert.Equal(3, cache.Count);
        }

        [Fact]
        public void GetEnumerator_AllowsIteration()
        {
            var lookups = new[]
            {
                new TestLookup("id1", "Name1"),
                new TestLookup("id2", "Name2")
            };
            var cache = new LookupCache<TestDomainEntity, TestLookup>(lookups);

            var count = 0;
            foreach (var lookup in cache)
            {
                count++;
                Assert.NotNull(lookup);
            }

            Assert.Equal(2, count);
        }

        [Fact]
        public void Cache_IsThreadSafe_ForReads()
        {
            var lookups = Enumerable.Range(1, 100)
                .Select(i => new TestLookup($"id{i}", $"Name{i}"))
                .ToArray();
            var cache = new LookupCache<TestDomainEntity, TestLookup>(lookups);

            var tasks = Enumerable.Range(1, 10).Select(i =>
                System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 1; j <= 100; j++)
                    {
                        var lookup = cache.FindById(
                            new DomainEntityIdentity<TestDomainEntity>($"id{j}"));
                        Assert.NotNull(lookup);
                    }
                })).ToArray();

            System.Threading.Tasks.Task.WaitAll(tasks);
        }
    }
}

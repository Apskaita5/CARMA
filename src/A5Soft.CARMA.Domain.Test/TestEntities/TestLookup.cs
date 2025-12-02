using System;

namespace A5Soft.CARMA.Domain.Test.TestEntities
{
    // Test lookup implementations
    public class TestLookup : LookupBase<TestDomainEntity>
    {
        public TestLookup(string id, string name)
        {
            _id = new DomainEntityIdentity<TestDomainEntity>(id);
            Name = name;
        }

        public string Name { get; }

        public override bool Match(string searchString)
        {
            return Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}

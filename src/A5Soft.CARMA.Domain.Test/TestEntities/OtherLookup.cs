using System;

namespace A5Soft.CARMA.Domain.Test.TestEntities
{
    public class OtherLookup : LookupBase<TestChildEntity>
    {
        public OtherLookup(string id, string description)
        {
            _id = new DomainEntityIdentity<TestChildEntity>(id);
            Description = description;
        }

        public string Description { get; }

        public override bool Match(string searchString)
        {
            return Description?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}

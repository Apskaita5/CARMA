using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Test
{
    internal class MockSimpleLookup : LookupBase<SimpleDomainEntity>
    {
        public MockSimpleLookup(string id)
        {
            _id = id;
            Name = "Lookup name";
        }


        public string Name { get; }

        public override bool Match(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString)) return true;
            if (string.IsNullOrWhiteSpace(Name)) return false;
            return Name.Contains(searchString, StringComparison.CurrentCultureIgnoreCase);
        }

        public override string ToString()
        {
            return Id?.Key ?? string.Empty;
        }
    }
}

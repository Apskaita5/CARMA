using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Test
{
    internal class SimpleLookup : ILookup<SimpleDomainEntity>
    {
        public SimpleLookup(string id)
        {
            Id = id;
            Name = "Lookup name";
        }
       
        public string Name { get; }
        public DomainEntityIdentity<SimpleDomainEntity> Id { get; }

        public override string ToString()
        {
            return Id?.Key ?? string.Empty;
        }
    }
}

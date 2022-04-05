﻿using A5Soft.CARMA.Domain.Metadata.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Test
{
    internal class SimpleDomainEntity : DomainObject<SimpleDomainEntity>, ISimpleDomainEntity
    {
        private SimpleLookup _lookup;
        private string _name;



        public SimpleDomainEntity(Rules.IValidationEngineProvider validationEngineProvider) 
            : base(validationEngineProvider) 
        {
            _lookup = new SimpleLookup("lt");
            _name = "test name";
        }

        public SimpleLookup Lookup
        {
            get => _lookup;
            set => SetLookupPropertyValue<SimpleLookup, SimpleDomainEntity>(nameof(Lookup), ref _lookup, value);
        }

        [IgnorePropertyMetadata]
        string ISimpleDomainEntity.Lookup => Lookup?.Id;

        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Test
{
    public interface ISimpleDomainEntity
    {
        [ExactLength(2)]
        [Required]
        string Lookup { get; }

        [Required]
        string Name { get; }
    }
}

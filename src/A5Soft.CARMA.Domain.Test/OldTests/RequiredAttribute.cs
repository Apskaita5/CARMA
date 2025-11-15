using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules;
using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Test
{
    internal class RequiredAttribute : RequiredAttributeBase
    {
        public RequiredAttribute() : base() {}

        protected override string GetLocalizedErrorMessageFor(string localizedPropName)
        {
            return $"{localizedPropName} is not specified.";
        }
    }
}

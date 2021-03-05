using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization
{
    public static class Extensions
    {

        internal static string GetClaimsDescription(this ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            return string.Join(", ", identity.Claims.Select(c => $"{c.ValueType}={c.Value}"));
        }

        internal static string GetUserName(this ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            if (identity.IsAuthenticated) return identity.Name;
            return "Unauthenticated User";
        }

    }
}

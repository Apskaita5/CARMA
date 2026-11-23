using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// A user identity as it crosses the data portal. On the way to the server it is
    /// only a claim about who the caller is, until the server has verified its signature.
    /// </summary>
    public sealed class IdentityEnvelope
    {
        /// <summary>Gets or sets an authentication type of the identity.</summary>
        public string AuthenticationType { get; set; }

        /// <summary>Gets or sets a claim type used for the identity name.</summary>
        public string NameType { get; set; }

        /// <summary>Gets or sets a claim type used for the identity roles.</summary>
        public string RoleType { get; set; }

        /// <summary>Gets or sets the claims of the identity (the signature included).</summary>
        public List<ClaimEnvelope> Claims { get; set; } = new List<ClaimEnvelope>();


        /// <summary>
        /// Creates an envelope for the identity given; null for a null identity.
        /// </summary>
        public static IdentityEnvelope From(ClaimsIdentity identity)
        {
            if (null == identity) return null;

            return new IdentityEnvelope
            {
                AuthenticationType = identity.AuthenticationType,
                NameType = identity.NameClaimType,
                RoleType = identity.RoleClaimType,
                Claims = identity.Claims
                    .Select(c => new ClaimEnvelope { Type = c.Type, Value = c.Value })
                    .ToList()
            };
        }

        /// <summary>
        /// Creates an identity from the envelope data.
        /// </summary>
        public ClaimsIdentity ToIdentity()
        {
            var claims = (Claims ?? new List<ClaimEnvelope>())
                .Where(c => null != c && !string.IsNullOrEmpty(c.Type))
                .Select(c => new Claim(c.Type, c.Value ?? string.Empty));

            return new ClaimsIdentity(claims, AuthenticationType,
                string.IsNullOrEmpty(NameType) ? ClaimsIdentity.DefaultNameClaimType : NameType,
                string.IsNullOrEmpty(RoleType) ? ClaimsIdentity.DefaultRoleClaimType : RoleType);
        }
    }
}

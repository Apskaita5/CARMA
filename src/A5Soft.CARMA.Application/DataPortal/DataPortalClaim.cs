using System;
using System.Security.Claims;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A container for <see cref="System.Security.Claims.Claim"/> serialization within <see cref="DataPortalRequest"/>.
    /// </summary>
    [Serializable]
    public class DataPortalClaim
    {
        /// <summary>
        /// new (empty instance)
        /// </summary>
        public DataPortalClaim() {}

        /// <summary>
        /// new instance that contains the data of the <paramref name="claim"/>
        /// </summary>
        /// <param name="claim">a claim to create a new DataPortalClaim for</param>
        public DataPortalClaim(Claim claim)
        {
            if (null == claim) throw new ArgumentNullException(nameof(claim));

            ClaimType = claim.Type;
            ClaimValue = claim.Value;
            ClaimValueType = claim.ValueType;
            Issuer = claim.Issuer;
            OriginalIssuer = claim.OriginalIssuer;
        }


        /// <inheritdoc cref="Claim.Type"/>
        public string ClaimType { get; set; }

        /// <inheritdoc cref="Claim.Value"/>
        public string ClaimValue { get; set; }

        /// <inheritdoc cref="Claim.ValueType"/>
        public string ClaimValueType { get; set; }

        /// <inheritdoc cref="Claim.Issuer"/>
        public string Issuer { get; set; }

        /// <inheritdoc cref="Claim.OriginalIssuer"/>
        public string OriginalIssuer { get; set; }


        /// <summary>
        /// Gets a new <see cref="Claim"/> using the serialized data. 
        /// </summary>
        public Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue, ClaimValueType, Issuer, OriginalIssuer);
        }

    }
}

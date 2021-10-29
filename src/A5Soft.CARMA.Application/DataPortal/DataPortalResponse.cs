using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A serializable wrapper for the data portal result.
    /// </summary>
    [Serializable]
    internal class DataPortalResponse
    {
        public DataPortalResponse() { }

        public DataPortalResponse(object result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        public DataPortalResponse(Exception processingException)
        {
            ProcessingException = processingException ?? throw new ArgumentNullException(nameof(processingException));
        }

        public DataPortalResponse(object result, ClaimsIdentity identity) : this(result)
        {
            SetIdentity(identity);
        }

        public DataPortalResponse(ClaimsIdentity identity)
        {
            SetIdentity(identity);
        }


        /// <summary>
        /// Gets or sets an exception that was thrown while processing the request on server (if any).
        /// </summary>
        public Exception ProcessingException { get; set; } = null;

        /// <summary>
        /// Gets or sets a result of the method invocation on server (if the method is not void).
        /// </summary>
        public object Result { get; set; } = null;

        /// <summary>
        /// Gets or sets a list of claims for the user who invokes the remote method.
        /// </summary>
        public List<DataPortalClaim> IdentityClaims { get; set; } = new List<DataPortalClaim>();

        /// <summary>
        /// Gets or sets an authentication type for the user who invokes the remote method.
        /// </summary>
        public string AuthenticationType { get; set; } = string.Empty;


        /// <summary>
        /// Gets a user identity returned by the use case if it is different from the initial identity.
        /// </summary>
        /// <param name="initialIdentity">initial user identity sent by a data portal request</param>
        /// <returns>a user identity returned by the use case</returns>
        public ClaimsIdentity GetUpdatedIdentity(ClaimsIdentity initialIdentity)
        {
            ClaimsIdentity result;
            if (AuthenticationType.IsNullOrWhiteSpace() || null == IdentityClaims || IdentityClaims.Count < 1)
                result = new ClaimsIdentity();
            else result = new ClaimsIdentity(IdentityClaims
                .Select(p => p.ToClaim()), AuthenticationType);
            
            if (!result.IsSameIdentity(initialIdentity)) return result;

            return null;
        }

        private void SetIdentity(ClaimsIdentity identity)
        {
            if (null != identity && identity.IsAuthenticated)
            {
                IdentityClaims = identity.Claims
                    .Select(c => new DataPortalClaim(c))
                    .ToList();
                AuthenticationType = identity.AuthenticationType;
            }
        }
    }
}

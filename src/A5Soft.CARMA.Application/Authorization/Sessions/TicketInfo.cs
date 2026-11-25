using System;

namespace A5Soft.CARMA.Application.Authorization.Sessions
{
    /// <summary>
    /// What a valid ticket says, as signed by the identity store that issued it.
    /// </summary>
    public sealed class TicketInfo
    {
        /// <summary>Creates the ticket information.</summary>
        public TicketInfo(string ticketId, string userId, string tenantId, string identityStamp,
            DateTimeOffset issuedAt, DateTimeOffset expiresAt)
        {
            TicketId = string.IsNullOrWhiteSpace(ticketId) ? throw new ArgumentNullException(nameof(ticketId)) : ticketId;
            UserId = string.IsNullOrWhiteSpace(userId) ? throw new ArgumentNullException(nameof(userId)) : userId;
            TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
            IdentityStamp = identityStamp;
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
        }

        /// <summary>Gets the identifier of the session: the same for every ticket issued for it.</summary>
        public string TicketId { get; }

        /// <summary>Gets the user's identifier.</summary>
        public string UserId { get; }

        /// <summary>Gets the tenant of the session; null for none.</summary>
        public string TenantId { get; }

        /// <summary>Gets the stamp of the identity when the ticket was issued.</summary>
        public string IdentityStamp { get; }

        /// <summary>Gets when the ticket was issued.</summary>
        public DateTimeOffset IssuedAt { get; }

        /// <summary>Gets when the ticket expires.</summary>
        public DateTimeOffset ExpiresAt { get; }
    }
}

using System;

namespace A5Soft.CARMA.Application.Authorization.Identity
{
    /// <summary>
    /// The identity of a scope has changed: the user data (a new stamp) or the session state other than the ticket.
    /// A new ticket alone is not reported.
    /// </summary>
    public sealed class IdentityChangedEventArgs : EventArgs
    {
        public IdentityChangedEventArgs(CarmaIdentity oldIdentity, CarmaIdentity newIdentity)
        {
            OldIdentity = oldIdentity;
            NewIdentity = newIdentity;
        }

        /// <summary>Gets the identity before the change.</summary>
        public CarmaIdentity OldIdentity { get; }

        /// <summary>Gets the identity after the change: the scope's identity from now on.</summary>
        public CarmaIdentity NewIdentity { get; }
    }
}

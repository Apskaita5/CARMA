using System;

namespace A5Soft.CARMA.Application.Background.Events
{
    /// <summary>
    /// Marks an event handler (<see cref="IAppEventHandler{TEvent}"/>) that runs with the identity of the user who
    /// published the event, fetched again when it runs (<see cref="Authorization.Sessions.IIdentityProvider.FetchAsync"/>),
    /// with the permissions the user has then. Without it a handler runs with the system identity.
    /// </summary>
    /// <remarks>
    /// <para>An event published without a user (in an anonymous scope, by the system identity, or by a user who has
    /// not passed the second factor yet) creates no work item for such a handler: the handler is skipped.</para>
    /// <para>A user who is disabled or deleted by the time the handler runs fails the work item, without a retry.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RunAsPublishingUserAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets whether to upgrade the user's identity to an administrator
        /// (<see cref="Authorization.Identity.CarmaIdentity.IsElevated"/>): the handler may do whatever it needs, while
        /// the audit trail still names the user. Default: false.
        /// </summary>
        public bool AsAdmin { get; set; }
    }
}

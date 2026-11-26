using System;

namespace A5Soft.CARMA.Domain.Events
{
    /// <summary>
    /// A base for application events: an identifier and the time the event occurred.
    /// </summary>
    /// <remarks>
    /// <para>A derived event is immutable: read-only properties, a public constructor for a new event (calling
    /// <see cref="AppEventBase()"/>) and a private constructor marked with
    /// <see cref="SerializationConstructorAttribute"/> taking every property, its own and
    /// <c>eventId</c> and <c>occurredOn</c>, calling <see cref="AppEventBase(Guid, DateTime)"/>.</para>
    /// <example><code>
    /// [BackgroundKey("claims.approved")]
    /// public sealed class ClaimApproved : AppEventBase
    /// {
    ///     public ClaimApproved(string claimId) { ClaimId = claimId; }
    ///
    ///     [SerializationConstructor]
    ///     private ClaimApproved(Guid eventId, DateTime occurredOn, string claimId) : base(eventId, occurredOn)
    ///     { ClaimId = claimId; }
    ///
    ///     public string ClaimId { get; }
    /// }
    /// </code></example>
    /// </remarks>
    public abstract class AppEventBase : IAppEvent
    {
        /// <summary>
        /// Creates a new event: a new identifier, occurring now.
        /// </summary>
        protected AppEventBase()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Recreates an event: for the serialization constructor of a derived event.
        /// </summary>
        /// <param name="eventId">the identifier of the event</param>
        /// <param name="occurredOn">the time the event occurred (UTC)</param>
        protected AppEventBase(Guid eventId, DateTime occurredOn)
        {
            EventId = eventId;
            OccurredOn = occurredOn.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(occurredOn, DateTimeKind.Utc)
                : occurredOn.ToUniversalTime();
        }


        /// <inheritdoc />
        public Guid EventId { get; }

        /// <inheritdoc />
        public DateTime OccurredOn { get; }
    }
}

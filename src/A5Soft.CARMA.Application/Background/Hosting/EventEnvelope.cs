using A5Soft.CARMA.Application.Background.Internal;
using Newtonsoft.Json;
using System;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// A stored work item: one event for one handler, for which tenant and (if the handler runs as the publishing
    /// user) for which user to handle it. Created by
    /// <see cref="AppEventContainer"/>; stored and fired by the <see cref="IBackgroundScheduler"/>; handled by the
    /// <see cref="IBackgroundWorkRunner"/>.
    /// </summary>
    /// <remarks>A scheduler stores it as one opaque string (<see cref="ToWire"/>, read back by
    /// <see cref="FromWire"/>), whatever its store's serializer. The properties are for naming and describing the
    /// work item in the scheduler's own store or dashboard; immutable.</remarks>
    public sealed class EventEnvelope
    {
        private string _wire;


        [JsonConstructor]
        private EventEnvelope() { }

        internal EventEnvelope(string eventKey, string handlerKey, string tenantId, string userId,
            string correlationId, string payload)
        {
            Id = Guid.NewGuid();
            EventKey = eventKey;
            HandlerKey = handlerKey;
            TenantId = tenantId;
            UserId = userId;
            CorrelationId = correlationId;
            CreatedUtc = DateTime.UtcNow;
            Payload = payload;
        }


        /// <summary>Gets the identifier of the work item: the same for all its attempts.</summary>
        [JsonProperty("i")]
        public Guid Id { get; private set; }

        /// <summary>Gets the key of the event type.</summary>
        [JsonProperty("e")]
        public string EventKey { get; private set; }

        /// <summary>Gets the key of the handler.</summary>
        [JsonProperty("h")]
        public string HandlerKey { get; private set; }

        /// <summary>Gets the tenant to handle the event for; null for none.</summary>
        [JsonProperty("t")]
        public string TenantId { get; private set; }

        /// <summary>
        /// Gets the user who published the event, for a handler that runs as the publishing user; null for a handler
        /// that runs as the system identity. The handler decides whom it runs as when it runs
        /// (<see cref="RunAsPublishingUserAttribute"/>).
        /// </summary>
        [JsonProperty("u")]
        public string UserId { get; private set; }

        /// <summary>
        /// Gets the correlation id: shared by every work item of one <see cref="AppEventContainer"/>, and in the log
        /// entries of their failures.
        /// </summary>
        [JsonProperty("c")]
        public string CorrelationId { get; private set; }

        /// <summary>Gets the time the event was published (UTC).</summary>
        [JsonProperty("d")]
        public DateTime CreatedUtc { get; private set; }

        /// <summary>Gets the number of attempts made before this one: 0 for the first.</summary>
        [JsonProperty("n")]
        public int Attempt { get; private set; }

        /// <summary>Gets the event, serialized.</summary>
        [JsonProperty("p")]
        public string Payload { get; private set; }

        /// <summary>Gets the size of the work item as stored (<see cref="ToWire"/>), in UTF-8 bytes.</summary>
        [JsonIgnore]
        public int WireSize => WireFormat.GetSize(ToWire());


        /// <summary>
        /// Gets the work item as one string, to store.
        /// </summary>
        public string ToWire() => _wire ?? (_wire = WireFormat.Write(this));

        /// <summary>
        /// Reads a work item stored by <see cref="ToWire"/>.
        /// </summary>
        /// <exception cref="FormatException">the string is not an event work item</exception>
        public static EventEnvelope FromWire(string wire)
        {
            var result = WireFormat.Read<EventEnvelope>(wire);
            if (string.IsNullOrWhiteSpace(result.EventKey) || string.IsNullOrWhiteSpace(result.HandlerKey)
                || null == result.Payload) throw new FormatException("A work item is not an event work item.");
            return result;
        }

        /// <summary>
        /// Gets the same work item for its next attempt.
        /// </summary>
        public EventEnvelope NextAttempt()
        {
            var result = (EventEnvelope)MemberwiseClone();
            result._wire = null;
            result.Attempt = Attempt + 1;
            return result;
        }

        /// <inheritdoc />
        public override string ToString()
            => $"event {EventKey} for handler {HandlerKey}{(null == TenantId ? string.Empty : $", tenant {TenantId}")}" +
               $"{(null == UserId ? string.Empty : $", user {UserId}")}, work item {Id:N}, attempt {Attempt + 1}";
    }
}

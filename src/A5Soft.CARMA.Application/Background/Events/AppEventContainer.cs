using A5Soft.CARMA.Application.Background.Hosting;
using A5Soft.CARMA.Application.Background.Internal;
using A5Soft.CARMA.Domain.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.Events
{
    /// <summary>
    /// The events of one transaction: added while it is in progress, handed to the background work provider by
    /// <see cref="FlushAsync"/> after it has committed. Created by <see cref="IAppEventPublisher.CreateContainer"/>.
    /// </summary>
    /// <remarks>
    /// <para>Holds the tenant, the publishing user and a correlation id, captured when it was created.</para>
    /// <para>Thread safe: events may be added from parallel tasks. Flushed once: adding or flushing after the flush
    /// throws, so a container belongs to one transaction.</para>
    /// <para>Each event becomes one work item per handler (<see cref="EventEnvelope"/>), so that handlers fail and
    /// retry independently.</para>
    /// </remarks>
    public sealed class AppEventContainer
    {
        private readonly object _lock = new object();
        private readonly List<PreparedEvent> _prepared = new List<PreparedEvent>();
        private readonly BackgroundWorkRegistry _registry;
        private readonly IBackgroundScheduler _scheduler;
        private readonly ILogger _logger;
        private bool _flushed;


        internal AppEventContainer(BackgroundWorkRegistry registry, IBackgroundScheduler scheduler, ILogger logger,
            string tenantId, string userId)
        {
            _registry = registry;
            _scheduler = scheduler;
            _logger = logger;
            TenantId = tenantId;
            UserId = userId;
            CorrelationId = WireFormat.NewId();
        }


        /// <summary>Gets the tenant the events are handled for; null for none.</summary>
        public string TenantId { get; }

        /// <summary>
        /// Gets the user who publishes the events, for the handlers that run as the publishing user
        /// (<see cref="RunAsPublishingUserAttribute"/>); null for none: those handlers are then skipped.
        /// </summary>
        public string UserId { get; }

        /// <summary>Gets the correlation id of the container's work items.</summary>
        public string CorrelationId { get; }

        /// <summary>Gets the number of work items waiting for the flush.</summary>
        public int Count
        {
            get { lock (_lock) return _prepared.Count; }
        }

        /// <summary>Gets whether the container has been flushed.</summary>
        public bool IsFlushed
        {
            get { lock (_lock) return _flushed; }
        }


        /// <summary>
        /// Adds an event: serializes it and prepares a work item for each of its handlers. A handler that runs as the
        /// publishing user is skipped when the container has none (<see cref="UserId"/>); an event without handlers
        /// is dropped. Call it while the transaction is in progress, so that a failure here rolls it back.
        /// </summary>
        /// <param name="appEvent">the event</param>
        /// <param name="runAtUtc">when to handle it (UTC); null for at once</param>
        /// <exception cref="InvalidOperationException">the container has been flushed; the event type has no
        /// <see cref="BackgroundKeyAttribute"/> (or was not scanned); or a work item is larger than the scheduler
        /// takes (<see cref="IBackgroundScheduler.MaxEnvelopeSize"/>)</exception>
        public void Add(IAppEvent appEvent, DateTime? runAtUtc = null)
        {
            if (null == appEvent) throw new ArgumentNullException(nameof(appEvent));
            ThrowIfFlushed();

            var eventType = appEvent.GetType();
            var eventKey = _registry.GetEventKey(eventType);
            var handlers = _registry.GetHandlers(eventType);
            if (handlers.Count < 1) return;

            var dueUtc = ToUtc(runAtUtc);
            var payload = AppEventSerializer.Serialize(appEvent);
            var maxSize = _scheduler.MaxEnvelopeSize;

            var prepared = new List<PreparedEvent>(handlers.Count);
            foreach (var handler in handlers)
            {
                if (handler.RunsAsPublishingUser && null == UserId)
                {
                    _logger.LogDebug($"Handler {handler.Key} is skipped for event {eventKey}: it runs as the publishing " +
                        "user, and the event is published without one.");
                    continue;
                }

                // the user only where the handler needs it
                var envelope = new EventEnvelope(eventKey, handler.Key, TenantId,
                    handler.RunsAsPublishingUser ? UserId : null, CorrelationId, payload);

                if (maxSize > 0)
                {
                    var size = envelope.WireSize;
                    if (size > maxSize) throw new InvalidOperationException(
                        $"Event {eventType.FullName} ({eventKey}) takes {size} bytes as a work item for handler " +
                        $"{handler.Key}; the scheduler takes at most {maxSize}. Carry less data in the event, e.g. " +
                        "identifiers rather than whole documents.");
                }

                prepared.Add(new PreparedEvent(envelope, dueUtc));
            }

            lock (_lock)
            {
                ThrowIfFlushedLocked();
                _prepared.AddRange(prepared);
            }
        }

        /// <summary>
        /// Hands every event added to the background work provider: call it once, after the transaction has
        /// committed. Not cancellable: the transaction has committed, so its events must go out whoever has stopped
        /// waiting.
        /// </summary>
        /// <remarks>Throws only when misused. A work item the provider fails to take is logged as critical, with the
        /// string to enqueue it by hand, and the others still go out.</remarks>
        /// <exception cref="InvalidOperationException">the container has been flushed already</exception>
        public async Task FlushAsync()
        {
            PreparedEvent[] batch;
            lock (_lock)
            {
                ThrowIfFlushedLocked();
                _flushed = true;
                batch = _prepared.ToArray();
                _prepared.Clear();
            }

            foreach (var item in batch)
            {
                try
                {
                    await _scheduler.EnqueueAsync(item.Envelope, item.DueUtc, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"The background work provider has failed to take {item.Envelope}, " +
                        $"correlation id {CorrelationId}: the event is lost unless enqueued by hand, due " +
                        $"{item.DueUtc:O}, work item: {item.Envelope.ToWire()}", ex);
                }
            }
        }


        private void ThrowIfFlushed()
        {
            lock (_lock) ThrowIfFlushedLocked();
        }

        private void ThrowIfFlushedLocked()
        {
            if (_flushed) throw new InvalidOperationException(
                "The event container has been flushed: create a new container for another transaction.");
        }

        private static DateTime ToUtc(DateTime? value)
        {
            if (!value.HasValue) return DateTime.UtcNow;

            switch (value.Value.Kind)
            {
                case DateTimeKind.Local: return value.Value.ToUniversalTime();
                case DateTimeKind.Unspecified: return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                default: return value.Value;
            }
        }


        private sealed class PreparedEvent
        {
            public PreparedEvent(EventEnvelope envelope, DateTime dueUtc)
            {
                Envelope = envelope;
                DueUtc = dueUtc;
            }

            public EventEnvelope Envelope { get; }
            public DateTime DueUtc { get; }
        }
    }
}

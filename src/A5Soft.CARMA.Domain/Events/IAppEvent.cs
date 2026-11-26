using System;

namespace A5Soft.CARMA.Domain.Events
{
    /// <summary>
    /// An application event: a fact that has happened, handled in the background by any number of handlers
    /// (<c>IAppEventHandler&lt;TEvent&gt;</c> in A5Soft.CARMA.Application).
    /// </summary>
    /// <remarks>
    /// <para>A concrete event type is marked with <see cref="BackgroundKeyAttribute"/>: its stable name in the
    /// background work store.</para>
    /// <para>An event crosses a process boundary as JSON, by the data portal's serialization rules: public properties,
    /// read-only ones fed through a private constructor marked with <see cref="SerializationConstructorAttribute"/>.
    /// No <see cref="Type"/> values and no file content. Carry what the handlers need and no more (ids rather than
    /// whole documents): the scheduler limits the size of a stored work item.</para>
    /// <para>Deserialization is tolerant: a property added later gets its default value in an event stored before.
    /// For a change of meaning, give the event a new key and keep the old type and handler until the stored events
    /// are processed.</para>
    /// </remarks>
    public interface IAppEvent
    {
        /// <summary>
        /// Gets the identifier of the event.
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// Gets the time the event occurred (UTC).
        /// </summary>
        DateTime OccurredOn { get; }
    }
}

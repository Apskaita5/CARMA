using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.Events
{
    /// <summary>
    /// Handles application events of a type, in the background, after the transaction that published them.
    /// </summary>
    /// <remarks>
    /// <para>An implementation is marked with <see cref="BackgroundKeyAttribute"/> and
    /// <see cref="DefaultServiceImplementationAttribute"/> for this interface closed over its event type, usually
    /// for <see cref="BuildConfiguration.Backend"/>: e.g.
    /// <c>[DefaultServiceImplementation(typeof(IAppEventHandler&lt;ClaimApproved&gt;), BuildConfiguration.Backend)]</c>.
    /// A handler of a base type (or <see cref="IAppEvent"/> itself) receives every event deriving from it.</para>
    /// <para>Runs with the system identity, or, marked <see cref="RunAsPublishingUserAttribute"/>, with the identity
    /// of the user who published the event (skipped for an event published without one).</para>
    /// <para>Scoped: each event is handled in a new scope, whose identity (the tenant included) is set before the
    /// handler is created, so it may depend on scoped and tenant scoped services, <see cref="IAppEventPublisher"/>
    /// included.</para>
    /// <para>Delivery is at least once: a handler may receive the same event again (a retry, a crash), so it must be
    /// idempotent, e.g. by <see cref="IAppEvent.EventId"/>.</para>
    /// <para>A handler publishes further events through its own <see cref="AppEventContainer"/>, as a use case does;
    /// they are flushed only if it flushes them. They are published with the handler's identity: a handler running
    /// with the system identity publishes without a user, so the handlers of those events that run as the publishing
    /// user are skipped.</para>
    /// </remarks>
    /// <typeparam name="TEvent">the type of the events handled</typeparam>
    [Service(ServiceLifetime.Scoped, allowMultipleImplementations: true)]
    public interface IAppEventHandler<in TEvent> : IBackgroundWork where TEvent : IAppEvent
    {
        /// <summary>
        /// Handles an event.
        /// </summary>
        /// <param name="appEvent">the event; never null</param>
        /// <param name="cancellationToken">cancelled when the host shuts down: the event is then handled again later</param>
        Task HandleAsync(TEvent appEvent, CancellationToken cancellationToken);
    }
}

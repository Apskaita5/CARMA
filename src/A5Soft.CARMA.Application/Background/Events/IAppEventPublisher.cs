using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.Background.Events
{
    /// <summary>
    /// Creates containers for application events that are published after a successful transaction
    /// (<see cref="IApplicationContext.Events"/>).
    /// </summary>
    /// <remarks>
    /// <para>A container captures the current tenant, the publishing user and a new correlation id when it is
    /// created. Create it after any change of identity within the use case (login, logout). The publishing user is
    /// a user who has fully logged in; an anonymous scope, the system identity and a user who has not passed the
    /// second factor yet publish without one. Each handler decides whom it runs as
    /// (<see cref="RunAsPublishingUserAttribute"/>, otherwise the system identity); a handler that runs as the
    /// publishing user is skipped for an event published without one.</para>
    /// <para>Typical use: create a container, add events while the transaction is in progress, and flush it only
    /// after the transaction has committed:</para>
    /// <code>
    /// var events = Context.Events.CreateContainer();
    /// // save through the repositories; events.Add(...) wherever it fits, passing the container to helpers if needed
    /// await events.FlushAsync();      // reached only if the save succeeded
    /// </code>
    /// <para>An unflushed container is simply discarded, which is the correct outcome when the transaction fails.
    /// A container that is never flushed after a successful transaction silently loses its events.</para>
    /// <para>Apps with their own transaction abstraction (a unit of work, a transaction scope) can integrate the
    /// container into it: create the container when the transaction begins and flush it after the commit. Use
    /// cases then never handle the container's lifecycle themselves.</para>
    /// <para>Events are supported where use cases run locally: on a server and in a thick client. In a thin client
    /// <see cref="CreateContainer"/> throws <see cref="NotSupportedException"/>.</para>
    /// </remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IAppEventPublisher
    {
        /// <summary>
        /// Creates a container for the events of one transaction.
        /// </summary>
        /// <exception cref="NotSupportedException">no <see cref="IBackgroundScheduler"/> is registered: a thin
        /// client, or a server without a background work provider</exception>
        AppEventContainer CreateContainer();
    }
}

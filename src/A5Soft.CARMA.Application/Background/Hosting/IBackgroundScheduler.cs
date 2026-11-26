using A5Soft.CARMA.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// The background work provider (e.g. Quartz.NET, Hangfire, <see cref="InMemoryBackgroundScheduler"/>): stores
    /// work items and fires each at its time by passing it to the <see cref="IBackgroundWorkRunner"/> of a new scope.
    /// Everything else (identity, tenant, handler resolution, retry decisions, logging) is the runner's.
    /// </summary>
    /// <remarks>
    /// <para>For each work item fired, the provider:</para>
    /// <list type="number">
    /// <item>reads it back from its string (<see cref="EventEnvelope.FromWire"/>, <see cref="CronRun.FromWire"/>);
    /// one that cannot be read is logged as critical and dropped;</item>
    /// <item>creates a new scope (<see cref="IBackgroundScopeFactory"/>) and calls the runner from it, passing a token
    /// cancelled when the host shuts down;</item>
    /// <item>on <see cref="WorkStatus.Retry"/>, enqueues the next attempt (<see cref="EventEnvelope.NextAttempt"/>,
    /// <see cref="CronRun.NextAttempt"/>) at <see cref="WorkResult.RetryAtUtc"/>;</item>
    /// <item>on a cancellation at shutdown (the runner rethrows it), keeps the same attempt for after the restart.</item>
    /// </list>
    /// <para>Cron schedules are registered with the provider at startup from <see cref="IoC.IoCManagerBase.CronSchedules"/>:
    /// each occurrence fires <c>new CronRun(schedule.Key)</c>; runs of a schedule not marked
    /// <see cref="CronSchedule.AllowConcurrentRuns"/> must not overlap.</para>
    /// <para>No scheduler is registered in a thin client, and none by default on a server: a server registers its
    /// provider's (e.g. marked <c>[DefaultServiceImplementation(typeof(IBackgroundScheduler), BuildConfiguration.Server)]</c>).
    /// A thick client gets <see cref="InMemoryBackgroundScheduler"/>.</para>
    /// </remarks>
    [Service(ServiceLifetime.Singleton)]
    public interface IBackgroundScheduler
    {
        /// <summary>
        /// Gets the largest work item the store takes, in UTF-8 bytes of its string
        /// (<see cref="EventEnvelope.WireSize"/>); 0 for no limit. Checked when an event is added to a container.
        /// </summary>
        int MaxEnvelopeSize { get; }

        /// <summary>
        /// Stores an event work item, to fire at a time.
        /// </summary>
        /// <param name="envelope">the work item</param>
        /// <param name="runAtUtc">the time to fire it (UTC); a past time fires it at once</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        Task EnqueueAsync(EventEnvelope envelope, DateTime runAtUtc, CancellationToken cancellationToken);

        /// <summary>
        /// Stores a cron job run (a run for a tenant, or a retry), to fire at a time.
        /// </summary>
        /// <param name="run">the work item</param>
        /// <param name="runAtUtc">the time to fire it (UTC); a past time fires it at once</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        Task EnqueueAsync(CronRun run, DateTime runAtUtc, CancellationToken cancellationToken);
    }
}

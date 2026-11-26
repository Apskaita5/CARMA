using A5Soft.CARMA.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// Runs one work item in the current scope: called by the background work provider
    /// (<see cref="IBackgroundScheduler"/>) from a new scope for each work item fired.
    /// </summary>
    /// <remarks>
    /// <para>Sets the scope's identity (system or user, with the tenant and its plugins) before it resolves the
    /// handler or job, so that they may depend on scoped and tenant scoped services; invokes it; and classifies a
    /// failure: a transient one is retried at the time the work proposes
    /// (<see cref="IBackgroundWork.GetRetryTime"/>; <see cref="BackgroundRetry.DefaultPolicy"/> for a failure before
    /// the work is created, e.g. while fetching the identity), anything else is abandoned and logged as critical.
    /// Each failure is logged once.</para>
    /// <para>One work item per scope: the scope's identity is set once.</para>
    /// </remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IBackgroundWorkRunner
    {
        /// <summary>
        /// Handles an event work item.
        /// </summary>
        /// <param name="envelope">the work item</param>
        /// <param name="cancellationToken">cancelled when the host shuts down</param>
        /// <returns>what became of it</returns>
        /// <exception cref="OperationCanceledException">the host is shutting down: keep the same attempt for later</exception>
        Task<WorkResult> RunEventAsync(EventEnvelope envelope, CancellationToken cancellationToken);

        /// <summary>
        /// Runs a cron job work item: the firing of a schedule (for a job running per tenant, it enqueues one run per
        /// active tenant), or a run for a tenant.
        /// </summary>
        /// <param name="run">the work item</param>
        /// <param name="cancellationToken">cancelled when the host shuts down</param>
        /// <returns>what became of it</returns>
        /// <exception cref="OperationCanceledException">the host is shutting down: keep the same attempt for later</exception>
        Task<WorkResult> RunCronAsync(CronRun run, CancellationToken cancellationToken);
    }
}

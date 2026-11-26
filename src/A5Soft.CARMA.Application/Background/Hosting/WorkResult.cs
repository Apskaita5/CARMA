using System;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// What became of a work item (<see cref="IBackgroundWorkRunner"/>), for the scheduler to act on.
    /// </summary>
    public sealed class WorkResult
    {
        private WorkResult(WorkStatus status, DateTime? retryAtUtc)
        {
            Status = status;
            RetryAtUtc = retryAtUtc;
        }


        /// <summary>Gets the result of work done (or skipped as not applicable, e.g. a plugin no longer assigned).</summary>
        public static WorkResult Succeeded { get; } = new WorkResult(WorkStatus.Succeeded, null);

        /// <summary>Gets the result of work abandoned: logged as critical by the runner.</summary>
        public static WorkResult Failed { get; } = new WorkResult(WorkStatus.Failed, null);

        /// <summary>Gets the result of work to attempt again.</summary>
        /// <param name="retryAtUtc">the time of the next attempt (UTC)</param>
        public static WorkResult Retry(DateTime retryAtUtc) => new WorkResult(WorkStatus.Retry, retryAtUtc);


        /// <summary>Gets what became of the work item.</summary>
        public WorkStatus Status { get; }

        /// <summary>
        /// Gets the time of the next attempt (UTC) for <see cref="WorkStatus.Retry"/>: enqueue the work item's
        /// next attempt (<see cref="EventEnvelope.NextAttempt"/>, <see cref="CronRun.NextAttempt"/>) for then.
        /// </summary>
        public DateTime? RetryAtUtc { get; }
    }
}

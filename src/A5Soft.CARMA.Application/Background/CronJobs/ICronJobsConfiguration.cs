using System.Collections.Generic;

namespace A5Soft.CARMA.Application.Background.CronJobs
{
    /// <summary>
    /// The schedules of the cron jobs (<see cref="ICronJob"/>), from the server's app configuration (by default the
    /// <c>CronJobsConfiguration</c> section). A cron job without a schedule here never runs.
    /// </summary>
    public interface ICronJobsConfiguration
    {
        /// <summary>
        /// Gets the schedules; empty for none.
        /// </summary>
        IReadOnlyList<ICronJobConfiguration> CronJobs { get; }
    }
}

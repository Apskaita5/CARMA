using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.Background.CronJobs
{
    /// <summary>
    /// The framework's <see cref="ICronJobsConfiguration"/>, bound from the <c>CronJobsConfiguration</c> section of a
    /// server's app configuration:
    /// <code>
    /// "CronJobsConfiguration": {
    ///   "CronJobs": [
    ///     { "Key": "claims.late-debt-reminder", "Expression": "0 0 7 * * ?" },
    ///     { "Key": "admin.cleanup", "Expression": "0 30 2 * * ?", "Enabled": false }
    ///   ]
    /// }
    /// </code>
    /// </summary>
    public sealed class CronJobsConfiguration : ConfigurationBase, ICronJobsConfiguration
    {
        /// <inheritdoc />
        public override Type InterfaceType => typeof(ICronJobsConfiguration);

        /// <inheritdoc />
        public override BuildConfiguration ForBuildConfiguration => BuildConfiguration.Server;

        /// <summary>
        /// Gets or sets the schedules; null for none.
        /// </summary>
        public List<CronJobConfiguration> CronJobs { get; set; }

        /// <inheritdoc />
        IReadOnlyList<ICronJobConfiguration> ICronJobsConfiguration.CronJobs
            => (IReadOnlyList<ICronJobConfiguration>)CronJobs ?? Array.Empty<ICronJobConfiguration>();


        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">a schedule has no key or no expression, or two have the same key</exception>
        public override void Validate()
        {
            if (null == CronJobs) return;

            foreach (var job in CronJobs)
            {
                if (null == job || string.IsNullOrWhiteSpace(job.Key)) throw new InvalidOperationException(
                    $"{RootSection}: a cron job has no key.");
                if (string.IsNullOrWhiteSpace(job.Expression)) throw new InvalidOperationException(
                    $"{RootSection}: cron job {job.Key} has no cron expression.");
            }

            var duplicate = CronJobs.GroupBy(j => j.Key.Trim(), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (null != duplicate) throw new InvalidOperationException(
                $"{RootSection}: cron job {duplicate.Key} is scheduled more than once.");
        }
    }
}

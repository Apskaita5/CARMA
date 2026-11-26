using System;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// A cron job's schedule, for the background work provider to register at startup
    /// (<see cref="IoC.IoCManagerBase.CronSchedules"/>).
    /// </summary>
    public sealed class CronSchedule
    {
        internal CronSchedule(string key, string expression, bool allowConcurrentRuns)
        {
            Key = key;
            Expression = expression;
            AllowConcurrentRuns = allowConcurrentRuns;
        }


        /// <summary>Gets the key of the cron job: fire <c>new CronRun(Key)</c> on each occurrence.</summary>
        public string Key { get; }

        /// <summary>
        /// Gets the cron expression, as configured: in the provider's syntax and the server's time zone (or the
        /// provider's scheduler time zone). The provider validates it when it registers the schedule.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Gets whether the job's runs may overlap; when false the provider runs them one at a time (in a tenant based
        /// app, the runs for the tenants included).
        /// </summary>
        public bool AllowConcurrentRuns { get; }


        /// <inheritdoc />
        public override string ToString() => $"{Key}: {Expression}";
    }
}

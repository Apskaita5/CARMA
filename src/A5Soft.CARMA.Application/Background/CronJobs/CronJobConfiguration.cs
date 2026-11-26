namespace A5Soft.CARMA.Application.Background.CronJobs
{
    /// <summary>
    /// The schedule of one cron job, bound from the app configuration (<see cref="CronJobsConfiguration"/>).
    /// </summary>
    public sealed class CronJobConfiguration : ICronJobConfiguration
    {
        /// <inheritdoc cref="ICronJobConfiguration.Key"/>
        public string Key { get; set; }

        /// <inheritdoc cref="ICronJobConfiguration.Expression"/>
        public string Expression { get; set; }

        /// <inheritdoc cref="ICronJobConfiguration.Enabled"/>
        public bool Enabled { get; set; } = true;
    }
}

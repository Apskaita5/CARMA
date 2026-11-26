namespace A5Soft.CARMA.Application.Background.CronJobs
{
    /// <summary>
    /// The schedule of one cron job (<see cref="ICronJobsConfiguration"/>).
    /// </summary>
    public interface ICronJobConfiguration
    {
        /// <summary>
        /// Gets the key of the cron job (its <see cref="Domain.Events.BackgroundKeyAttribute"/>); a key that no cron
        /// job has stops startup.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the cron expression, in the background work provider's syntax and the server's time zone.
        /// </summary>
        string Expression { get; }

        /// <summary>
        /// Gets whether the schedule is used; a disabled job never runs.
        /// </summary>
        bool Enabled { get; }
    }
}

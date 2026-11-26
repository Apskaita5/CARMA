using A5Soft.CARMA.Application.Background.CronJobs;
using A5Soft.CARMA.Domain.Events;
using System;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// A cron job found by the startup scan (<see cref="BackgroundWorkRegistry"/>).
    /// </summary>
    public sealed class CronJobInfo
    {
        internal CronJobInfo(string key, Type jobType, string pluginId)
        {
            Key = key;
            JobType = jobType;
            PluginId = pluginId;
            IsGlobal = jobType.IsDefined(typeof(GlobalCronJobAttribute), false);
            AllowConcurrentRuns = jobType.IsDefined(typeof(AllowConcurrentRunsAttribute), false);
        }


        /// <summary>Gets the job's key (<see cref="BackgroundKeyAttribute"/>): the configuration names it by.</summary>
        public string Key { get; }

        /// <summary>Gets the job's type: registered under itself too, so that the runner resolves exactly it.</summary>
        public Type JobType { get; }

        /// <summary>Gets the identifier of the plugin the job comes from; null for the app's own.</summary>
        public string PluginId { get; }

        /// <summary>Gets whether the job runs once, without a tenant (<see cref="GlobalCronJobAttribute"/>).</summary>
        public bool IsGlobal { get; }

        /// <summary>Gets whether the job's runs may overlap (<see cref="AllowConcurrentRunsAttribute"/>).</summary>
        public bool AllowConcurrentRuns { get; }
    }
}

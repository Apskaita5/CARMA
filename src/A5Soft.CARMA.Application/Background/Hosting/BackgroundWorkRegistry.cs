using A5Soft.CARMA.Application.Background.CronJobs;
using A5Soft.CARMA.Application.Background.Events;
using A5Soft.CARMA.Application.IoC;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Application.Tenancy;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Events;
using A5Soft.CARMA.Domain.UnitTests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// The application event types, event handlers and cron jobs of the app and its plugins, by key. Built by
    /// <see cref="IoCManagerBase"/> from the startup scan and added to the container. Immutable; thread safe.
    /// </summary>
    public sealed class BackgroundWorkRegistry
    {
        private readonly Dictionary<string, Type> _eventTypes;
        private readonly Dictionary<Type, string> _eventKeys;
        private readonly Dictionary<string, AppEventHandlerInfo> _handlers;
        private readonly Dictionary<string, CronJobInfo> _cronJobs;
        private readonly ConcurrentDictionary<Type, IReadOnlyList<AppEventHandlerInfo>> _handlersByEvent
            = new ConcurrentDictionary<Type, IReadOnlyList<AppEventHandlerInfo>>();


        private BackgroundWorkRegistry(Dictionary<string, Type> eventTypes, Dictionary<string, AppEventHandlerInfo> handlers,
            Dictionary<string, CronJobInfo> cronJobs, IReadOnlyList<ApplicationServiceInfo> workServices)
        {
            _eventTypes = eventTypes;
            _eventKeys = eventTypes.ToDictionary(e => e.Value, e => e.Key);
            _handlers = handlers;
            _cronJobs = cronJobs;
            WorkServices = workServices;
        }


        /// <summary>Gets the event types, by key.</summary>
        public IReadOnlyDictionary<string, Type> EventTypes => _eventTypes;

        /// <summary>Gets the event handlers registered in this build.</summary>
        public IReadOnlyCollection<AppEventHandlerInfo> Handlers => _handlers.Values;

        /// <summary>Gets the cron jobs registered in this build.</summary>
        public IReadOnlyCollection<CronJobInfo> CronJobs => _cronJobs.Values;

        /// <summary>Gets the services of the handlers and cron jobs, to register each under its own type too.</summary>
        internal IReadOnlyList<ApplicationServiceInfo> WorkServices { get; }


        /// <summary>
        /// Gets the key of an event type.
        /// </summary>
        /// <exception cref="InvalidOperationException">the type has no key: it has no
        /// <see cref="BackgroundKeyAttribute"/>, or its assembly is not among the app's assemblies or plugins</exception>
        public string GetEventKey(Type eventType)
        {
            if (null == eventType) throw new ArgumentNullException(nameof(eventType));

            return _eventKeys.TryGetValue(eventType, out var key) ? key : throw new InvalidOperationException(
                $"Event type {eventType.FullName} is not registered: mark it with [BackgroundKey] in an assembly " +
                "that the app scans (one of its assemblies, or a plugin).");
        }

        /// <summary>Gets an event type by its key.</summary>
        public bool TryGetEventType(string key, out Type eventType)
        {
            eventType = null;
            return null != key && _eventTypes.TryGetValue(key, out eventType);
        }

        /// <summary>Gets a handler by its key.</summary>
        public bool TryGetHandler(string key, out AppEventHandlerInfo handler)
        {
            handler = null;
            return null != key && _handlers.TryGetValue(key, out handler);
        }

        /// <summary>Gets a cron job by its key.</summary>
        public bool TryGetCronJob(string key, out CronJobInfo cronJob)
        {
            cronJob = null;
            return null != key && _cronJobs.TryGetValue(key, out cronJob);
        }

        /// <summary>
        /// Gets the handlers of an event type: those declaring the type or a type it derives from (or an interface it
        /// implements), by key.
        /// </summary>
        public IReadOnlyList<AppEventHandlerInfo> GetHandlers(Type eventType)
        {
            if (null == eventType) throw new ArgumentNullException(nameof(eventType));

            return _handlersByEvent.GetOrAdd(eventType, t => _handlers.Values
                .Where(h => h.EventType.IsAssignableFrom(t))
                .OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
                .ToArray());
        }

        /// <summary>
        /// Gets the schedules of the cron jobs from the configuration: the enabled ones.
        /// </summary>
        /// <param name="configuration">the configuration; null for none (a build that is not a server)</param>
        /// <exception cref="InvalidOperationException">a schedule names a key that no cron job of this build has</exception>
        public IReadOnlyList<CronSchedule> GetSchedules(ICronJobsConfiguration configuration)
        {
            var result = new List<CronSchedule>();
            if (null == configuration?.CronJobs) return result;

            foreach (var entry in configuration.CronJobs.Where(e => null != e && e.Enabled))
            {
                var key = entry.Key?.Trim();
                if (!TryGetCronJob(key, out var job)) throw new InvalidOperationException(
                    $"The configuration schedules cron job {key}, which does not exist: the cron jobs are " +
                    $"{(_cronJobs.Count < 1 ? "none" : string.Join(", ", _cronJobs.Keys.OrderBy(k => k)))}.");

                result.Add(new CronSchedule(job.Key, entry.Expression.Trim(), job.AllowConcurrentRuns));
            }

            return result;
        }


        /// <summary>
        /// Builds the registry from the composed services and the scanned event types.
        /// </summary>
        /// <param name="services">every service of the build</param>
        /// <param name="eventTypes">the concrete event types scanned</param>
        /// <param name="plugins">the plugins accepted</param>
        /// <param name="buildConfiguration">the app build</param>
        /// <param name="isTenantBased">whether the app has tenant scoped services, i.e. is surely tenant based</param>
        /// <exception cref="InvalidOperationException">the message lists every violation</exception>
        internal static BackgroundWorkRegistry Create(IReadOnlyList<ApplicationServiceInfo> services,
            IEnumerable<Type> eventTypes, IEnumerable<PluginAssembly> plugins, BuildConfiguration buildConfiguration,
            bool isTenantBased)
        {
            var violations = new List<ConventionViolation>();
            var pluginIds = plugins.ToDictionary(p => p.Assembly, p => p.Id);
            string PluginOf(Type type) => pluginIds.TryGetValue(type.Assembly, out var id) ? id : null;

            var handlerServices = services.Where(s => null == s.ImplementationInstance && null != s.InterfaceType
                && s.InterfaceType.IsGenericType
                && s.InterfaceType.GetGenericTypeDefinition() == typeof(IAppEventHandler<>)).ToList();
            var cronServices = services.Where(s => null == s.ImplementationInstance
                && s.InterfaceType == typeof(ICronJob)).ToList();

            // event types: those scanned, and those the handlers declare (if concrete)
            var allEventTypes = eventTypes
                .Concat(handlerServices.Select(s => s.InterfaceType.GetGenericArguments()[0]))
                .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters)
                .Distinct();
            var events = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (var type in allEventTypes)
                AddByKey(events, type, type, "event type", violations);

            var handlers = new Dictionary<string, AppEventHandlerInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var service in handlerServices)
            {
                var type = service.ImplementationType;
                var key = GetKey(type, "event handler", violations);
                if (null == key) continue;

                var info = new AppEventHandlerInfo(key, type, service.InterfaceType.GetGenericArguments()[0], PluginOf(type));
                if (handlers.TryGetValue(key, out var existing)) violations.Add(new ConventionViolation(type, string.Empty,
                    $"has the background key {key} of event handler {existing.HandlerType.FullName}"));
                else handlers.Add(key, info);
            }

            var hasTenantDirectory = services.Any(s => s.InterfaceType == typeof(ITenantDirectory));
            var cronJobs = new Dictionary<string, CronJobInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var service in cronServices)
            {
                var type = service.ImplementationType;
                if (service.ForBuildConfiguration != BuildConfiguration.Server) violations.Add(new ConventionViolation(
                    type, string.Empty, $"is a cron job meant for {service.ForBuildConfiguration}: a cron job runs on a " +
                    "server only, mark it [DefaultServiceImplementation(typeof(ICronJob), BuildConfiguration.Server)]"));

                var key = GetKey(type, "cron job", violations);
                if (null == key) continue;

                var info = new CronJobInfo(key, type, PluginOf(type));
                if (isTenantBased && !info.IsGlobal && !hasTenantDirectory
                    && buildConfiguration.Serves(BuildConfiguration.Server)) violations.Add(new ConventionViolation(type,
                    string.Empty, $"runs per tenant, and the app is tenant based without an {nameof(ITenantDirectory)} " +
                    "to tell its tenants: implement one, or mark the job [GlobalCronJob]"));

                if (cronJobs.TryGetValue(key, out var existing)) violations.Add(new ConventionViolation(type, string.Empty,
                    $"has the background key {key} of cron job {existing.JobType.FullName}"));
                else cronJobs.Add(key, info);
            }

            if (violations.Count > 0) throw new InvalidOperationException(
                ConventionViolation.Format("Background work cannot be registered", violations));

            return new BackgroundWorkRegistry(events, handlers, cronJobs, handlerServices.Concat(cronServices).ToList());
        }

        private static void AddByKey<T>(Dictionary<string, T> into, Type type, T value, string what,
            List<ConventionViolation> violations)
        {
            var key = GetKey(type, what, violations);
            if (null == key) return;

            if (into.TryGetValue(key, out var existing)) violations.Add(new ConventionViolation(type, string.Empty,
                $"has the background key {key} of {what} {existing}"));
            else into.Add(key, value);
        }

        private static string GetKey(Type type, string what, List<ConventionViolation> violations)
        {
            var key = type.GetCustomAttribute<BackgroundKeyAttribute>(false)?.Key;
            if (null == key) violations.Add(new ConventionViolation(type, string.Empty,
                $"is an {what} without a key: mark it [BackgroundKey(\"...\")]"));
            return key;
        }
    }
}

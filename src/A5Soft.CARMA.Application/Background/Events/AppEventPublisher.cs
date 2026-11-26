using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Background.Hosting;
using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.Background.Events
{
    /// <summary>
    /// The framework's <see cref="IAppEventPublisher"/>.
    /// </summary>
    [DefaultServiceImplementation(typeof(IAppEventPublisher))]
    public sealed class AppEventPublisher : IAppEventPublisher
    {
        private readonly IAuthenticationStateProvider _authentication;
        private readonly BackgroundWorkRegistry _registry;
        private readonly ILogger _logger;
        private readonly IBackgroundScheduler _scheduler;


        /// <summary>
        /// Creates the publisher.
        /// </summary>
        /// <param name="authentication">the scope's identity</param>
        /// <param name="registry">the event types and handlers</param>
        /// <param name="logger">the logger</param>
        /// <param name="scheduler">the background work provider; null where there is none (a thin client)</param>
        public AppEventPublisher(IAuthenticationStateProvider authentication, BackgroundWorkRegistry registry,
            ILogger logger, IBackgroundScheduler scheduler = null)
        {
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scheduler = scheduler;
        }


        /// <inheritdoc />
        public AppEventContainer CreateContainer()
        {
            if (null == _scheduler) throw new NotSupportedException(
                $"Application events cannot be published here: no {nameof(IBackgroundScheduler)} is registered. " +
                "Events are published where use cases run locally (a server with a background work provider, or a " +
                "thick client), never in a thin client.");

            // only a user who has fully logged in can be impersonated: not the system, nor a pending second factor
            var identity = _authentication.Identity;
            var userId = identity.IsAuthenticated && !identity.IsSystem ? identity.UserId : null;

            return new AppEventContainer(_registry, _scheduler, _logger, identity.TenantId, userId);
        }
    }
}

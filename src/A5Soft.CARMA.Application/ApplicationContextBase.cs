using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Background.Events;
using A5Soft.CARMA.Application.Caching;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base for an app's own application context (an interface inheriting <see cref="IApplicationContext"/>):
    /// the application services, with the domain part delegated to the scope's domain context, so that both see
    /// the same validation provider.
    /// </summary>
    /// <remarks>Register the app's context under its own interface; never as <see cref="Domain.IDomainContext"/>,
    /// which the client data portal depends on: that would make a circular dependency.
    /// App's IApplicationContext shall always be a scoped service.</remarks>
    public abstract class ApplicationContextBase : IApplicationContext
    {
        private readonly Domain.IDomainContext _domain;


        /// <summary>
        /// Creates a new application context.
        /// </summary>
        /// <param name="domainContext">the scope's domain context</param>
        /// <param name="authorizationProvider">the authorization provider</param>
        /// <param name="dataPortal">the client data portal of the scope</param>
        /// <param name="authentication">the scope's identity</param>
        /// <param name="exceptionFactory">the app's factory of the localized exceptions the framework throws</param>
        /// <param name="lookups">the lookup service</param>
        /// <param name="clientCache">the scope's cache of values fetched from a remote server</param>
        /// <param name="events">the publisher of application events</param>
        /// <param name="logger">the logger</param>
        protected ApplicationContextBase(Domain.IDomainContext domainContext,
            IAuthorizationProvider authorizationProvider, IClientDataPortal dataPortal,
            IAuthenticationStateProvider authentication, IExceptionFactory exceptionFactory,
            ILookupService lookups, Caching.IClientCache clientCache,
            IAppEventPublisher events, ILogger logger)
        {
            _domain = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
            AuthorizationProvider = authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
            DataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            Authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            ExceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            Lookups = lookups ?? throw new ArgumentNullException(nameof(lookups));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ClientCache = clientCache ?? throw new ArgumentNullException(nameof(clientCache));
            Events = events ?? throw new ArgumentNullException(nameof(events));
        }


        /// <inheritdoc />
        public IMetadataProvider MetadataProvider => _domain.MetadataProvider;

        /// <inheritdoc />
        public IValidationEngineProvider ValidationProvider => _domain.ValidationProvider;

        /// <inheritdoc />
        public BindingMode BindingMode => _domain.BindingMode;

        /// <inheritdoc />
        public IAuthorizationProvider AuthorizationProvider { get; }

        /// <inheritdoc />
        public IClientDataPortal DataPortal { get; }

        /// <inheritdoc />
        public ILogger Logger { get; }

        /// <inheritdoc />
        public IAuthenticationStateProvider Authentication { get; }

        /// <inheritdoc />
        public IExceptionFactory ExceptionFactory { get; }

        /// <inheritdoc />
        public ILookupService Lookups { get; }

        /// <inheritdoc />
        public Caching.IClientCache ClientCache { get; }

        /// <inheritdoc />
        public IAppEventPublisher Events { get; }
    }
}

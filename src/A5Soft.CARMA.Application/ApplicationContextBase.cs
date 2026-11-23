using A5Soft.CARMA.Application.Authorization;
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
    /// <remarks>Register the app's context under its own interface; never as <see cref="Domain.IApplicationContext"/>,
    /// which the client data portal depends on: that would make a circular dependency.
    /// App's IApplicationContext shall always be a scoped service.</remarks>
    public abstract class ApplicationContextBase : IApplicationContext
    {
        private readonly Domain.IApplicationContext _domain;


        /// <summary>
        /// Creates a new application context.
        /// </summary>
        /// <param name="domainContext">the scope's domain context</param>
        /// <param name="authorizationProvider">the authorization provider</param>
        /// <param name="dataPortal">the client data portal of the scope</param>
        protected ApplicationContextBase(Domain.IApplicationContext domainContext,
            IAuthorizationProvider authorizationProvider, IClientDataPortal dataPortal)
        {
            _domain = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
            AuthorizationProvider = authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
            DataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
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
    }
}

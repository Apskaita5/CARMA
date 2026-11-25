using System;
using System.Collections.Concurrent;
using System.Linq;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// Provides the authorizers of the use cases from their interfaces' authorization attributes and the
    /// <see cref="PermissionCatalog"/>.
    /// </summary>
    [DefaultServiceImplementation(typeof(IAuthorizationProvider))]
    public class DefaultAuthorizationProvider : IAuthorizationProvider
    {
        private readonly ConcurrentDictionary<Type, IUseCaseAuthorizer> _cache
            = new ConcurrentDictionary<Type, IUseCaseAuthorizer>();
        private readonly PermissionCatalog _catalog;
        private readonly IAuthorizationExceptionProvider _exceptionProvider;


        /// <summary>Creates the provider.</summary>
        /// <param name="catalog">the permission catalog, holding an instance of every permission type</param>
        /// <param name="exceptionProvider">provider for localized exceptions</param>
        public DefaultAuthorizationProvider(PermissionCatalog catalog, IAuthorizationExceptionProvider exceptionProvider)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _exceptionProvider = exceptionProvider ?? throw new ArgumentNullException(nameof(exceptionProvider));
        }


        /// <inheritdoc />
        public IUseCaseAuthorizer GetAuthorizer(Type useCaseType)
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));
            return _cache.GetOrAdd(useCaseType, t => new UseCaseAuthorizer(FindInterface(t), _catalog, _exceptionProvider));
        }


        /// <summary>
        /// Finds the interface carrying the authorization rules: the type itself if it is an interface, otherwise the one
        /// interface it implements that declares them (or else its remote service interface).
        /// </summary>
        /// <param name="useCaseType">the use case interface or implementation</param>
        private static Type FindInterface(Type useCaseType)
        {
            if (useCaseType.IsInterface) return useCaseType;

            var declaring = useCaseType.GetInterfaces().Where(IsDeclaring).ToList();
            if (declaring.Count == 1) return declaring[0];
            if (declaring.Count > 1) throw new InvalidOperationException(
                $"Use case {useCaseType.FullName} implements more than one interface declaring authorization: " +
                $"{string.Join(", ", declaring.Select(i => i.FullName))}.");

            var remote = useCaseType.GetInterfaces().Where(i => i.IsDefined(typeof(RemoteServiceAttribute), true)).ToList();
            if (remote.Count == 1) return remote[0];

            throw new InvalidOperationException($"Type {useCaseType.FullName} is not a use case.");
        }

        /// <summary>
        /// Gets a value indicating whether an interface carries an authorization attribute.
        /// </summary>
        /// <param name="type">the interface</param>
        private static bool IsDeclaring(Type type)
            => type.IsDefined(typeof(AuthorizeAttribute), false)
                || type.IsDefined(typeof(AllowAnonymousAttribute), false)
                || type.IsDefined(typeof(AllowSecondFactorPendingAttribute), false)
                || type.IsDefined(typeof(AllowAdminOnlyAttribute), false);
    }
}

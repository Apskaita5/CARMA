using A5Soft.CARMA.Application.Authorization.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// The authorization rules of a use case, read from its interface.
    /// </summary>
    public sealed class UseCaseAuthorizer : IUseCaseAuthorizer
    {
        /// <summary>Why the declaration is not valid (no attribute, or more than one); null if it is.</summary>
        private readonly string _misdeclared;
        private readonly IExceptionFactory _exceptions;


        /// <summary>
        /// Reads the authorization rules of a use case interface.
        /// </summary>
        /// <param name="useCaseInterface">the use case interface carrying the authorization attributes</param>
        /// <param name="catalog">the permission catalog, holding an instance of every permission type</param>
        /// <param name="exceptions">the app's factory of localized exceptions</param>
        /// <exception cref="ArgumentNullException">a parameter is null</exception>
        public UseCaseAuthorizer(Type useCaseInterface, PermissionCatalog catalog, IExceptionFactory exceptions)
        {
            UseCaseType = useCaseInterface ?? throw new ArgumentNullException(nameof(useCaseInterface));
            _exceptions = exceptions ?? throw new ArgumentNullException(nameof(exceptions));
            if (null == catalog) throw new ArgumentNullException(nameof(catalog));

            var authorize = useCaseInterface.GetCustomAttribute<AuthorizeAttribute>(false);
            AllowsAnonymous = useCaseInterface.IsDefined(typeof(AllowAnonymousAttribute), false);
            AllowsSecondFactorPending = useCaseInterface.IsDefined(typeof(AllowSecondFactorPendingAttribute), false);
            AllowsAdminOnly = useCaseInterface.IsDefined(typeof(AllowAdminOnlyAttribute), false);
            Permissions = authorize?.PermissionTypes.Select(catalog.Get).ToList() ?? new List<Permission>();

            var declared = (null == authorize ? 0 : 1) + (AllowsAnonymous ? 1 : 0) + (AllowsSecondFactorPending ? 1 : 0)
                + (AllowsAdminOnly ? 1 : 0);
            if (declared < 1) _misdeclared = $"Use case {UseCaseType.FullName} declares no authorization: mark it " +
                $"with {nameof(AuthorizeAttribute)}, {nameof(AllowAnonymousAttribute)}, " +
                $"{nameof(AllowSecondFactorPendingAttribute)} or {nameof(AllowAdminOnlyAttribute)}.";
            else if (declared > 1) _misdeclared = $"Use case {UseCaseType.FullName} declares more than one " +
                "authorization attribute.";
        }


        /// <inheritdoc />
        public Type UseCaseType { get; }

        /// <inheritdoc />
        public bool AllowsAnonymous { get; }

        /// <inheritdoc />
        public bool AllowsSecondFactorPending { get; }

        /// <inheritdoc />
        public bool AllowsAdminOnly { get; }

        /// <inheritdoc />
        public IReadOnlyList<Permission> Permissions { get; }


        /// <inheritdoc />
        public bool CanInvoke(CarmaIdentity identity) => null == Refuse(identity);

        /// <inheritdoc />
        public void EnsureCanInvoke(CarmaIdentity identity)
        {
            var refusal = Refuse(identity);
            if (null != refusal) throw refusal;
        }

        /// <summary>
        /// Gets the reason to refuse an identity, in the order: no authorization declared, anonymous, second factor
        /// pending, none of the permissions held.
        /// </summary>
        /// <param name="identity">the identity to check; null is treated as anonymous</param>
        /// <returns>the exception to throw; null if the identity may invoke the use case</returns>
        private Exception Refuse(CarmaIdentity identity)
        {
            if (null != _misdeclared) return new InvalidOperationException(_misdeclared);

            if (AllowsAnonymous) return null;

            if (null == identity || identity.IsAnonymous) return new NotAuthenticatedException();

            if (AllowsAdminOnly && !identity.IsAdmin) return _exceptions.AdminOnly() ??
                throw new InvalidOperationException($"{nameof(IExceptionFactory)}.{nameof(IExceptionFactory.AdminOnly)} has returned no exception.");

            if (identity.IsSecondFactorPending) return AllowsSecondFactorPending
                ? null : new NotAuthenticatedException("Provide the second factor first.");

            if (Permissions.Count < 1 || identity.HasAnyPermission(Permissions)) return null;

            return _exceptions.NotAuthorized(Permissions) ??
                throw new InvalidOperationException($"{nameof(IExceptionFactory)}.{nameof(IExceptionFactory.NotAuthorized)} has returned no exception.");
        }
    }
}

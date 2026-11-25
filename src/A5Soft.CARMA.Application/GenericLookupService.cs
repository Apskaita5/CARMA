using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.Retry;
using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// The base of the app's <see cref="ILookupService"/>: fetches a lookup list if the requesting use case
    /// declares it and the scope's identity may invoke that use case.
    /// </summary>
    /// <remarks>Lookups (e.g. list of persons to be displayed in dropdowns) contain
    /// sensitive personal and/or business data. However there is no reasonable way
    /// to manage access authorization as the need of a particular lookup
    /// is defined by business entities that the user has access to.
    /// I.e. if a user has write access to invoices he needs a lookup for persons.
    /// Therefore we need a service that is only accessible by use cases
    /// which transfers authorization responsibility to them.
    /// E.g. save invoice use case shall be responsible for providing all the necessary lookups
    /// for a user in order to edit an invoice.</remarks>
    public abstract class GenericLookupService : ILookupService
    {
        private readonly IClientDataPortal _dataPortal;
        private readonly IAuthenticationStateProvider _authenticationStateProvider;


        /// <summary>
        /// Gets an authorization provider.
        /// </summary>
        protected IAuthorizationProvider AuthorizationProvider { get; }

        /// <summary>Gets the logger; null for none.</summary>
        protected ILogger Logger { get; }

        /// <inheritdoc />
        protected IBusinessExceptionFactory BusinessExceptions { get; }


        protected GenericLookupService(IAuthenticationStateProvider authenticationStateProvider,
            IAuthorizationProvider authorizationProvider, IClientDataPortal dataPortal,
            IBusinessExceptionFactory businessExceptions, ILogger logger)
        {
            _authenticationStateProvider = authenticationStateProvider
                ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            AuthorizationProvider = authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
            BusinessExceptions = businessExceptions ?? throw new ArgumentNullException(nameof(businessExceptions));
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            Logger = logger;
        }


        /// <summary>
        /// Gets a lookup value.
        /// </summary>
        /// <param name="requesterType">a type of the use case that requests the lookup</param>
        public async Task<List<TLookup>> FetchAsync<TLookup, TEntity>(Type requesterType, CancellationToken ct = default)
            where TLookup : LookupBase<TEntity>
            where TEntity : class, IDomainEntity<TEntity>
        {
            if (null == requesterType) throw new ArgumentNullException(nameof(requesterType));

            Logger?.LogMethodEntry(this.GetType(), nameof(FetchAsync), requesterType);

            List<TLookup> result;
            try
            {
                if (_dataPortal.IsRemote)
                {
                    result = GetValueFromLocalCache<TLookup, TEntity>();
                    if (null == result)
                    {
                        result = await _dataPortal.FetchAsync<Type, List<TLookup>>(GetType(), requesterType,
                            new[] { typeof(TLookup), typeof(TEntity) }, ct).ConfigureAwait(false);

                        SetLocalCacheValue<TLookup, TEntity>(result);
                    }
                }
                else
                {
                    await AuthorizeAsync<TLookup>(requesterType, ct).ConfigureAwait(false);

                    result = await DoFetchAsync<TLookup, TEntity>(ct).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                var toThrow = UseCaseBoundary.Handle(e, ct, Logger, BusinessExceptions,
                    GetType(), method);
                if (ReferenceEquals(toThrow, e)) throw;
                throw toThrow;
            }

            Logger?.LogMethodExit(this.GetType(), nameof(FetchAsync));

            return result;
        }

        /// <summary>
        /// Implement this method to get a requested value from a local cache (if used/configured).
        /// </summary>
        /// <remarks>Do not configure local cache for shared environments even if a remote data portal is used.
        /// The data portal will return only lookups for the user, which will render the
        /// shared cache invalid. Data portal cannot return lookup values that are not meant for the user
        /// as it will compromise its security.</remarks>
        protected abstract List<TLookup> GetValueFromLocalCache<TLookup, TEntity>()
            where TLookup : LookupBase<TEntity>
            where TEntity : class, IDomainEntity<TEntity>;

        /// <summary>
        /// Implement this method to add fetched lookup value to a local cache (if used/configured).
        /// </summary>
        /// <remarks>Only invoked if a remote data portal is used.
        /// Do not configure local cache for shared environments even if a remote data portal is used.
        /// The data portal might return only lookups for the user, which will render the
        /// shared cache invalid. Data portal cannot return lookup values that are not meant for the user
        /// as it will compromise its security.</remarks>
        protected abstract void SetLocalCacheValue<TLookup, TEntity>(List<TLookup> value)
            where TLookup : LookupBase<TEntity>
            where TEntity : class, IDomainEntity<TEntity>;

        /// <summary>
        /// Implement this method to fetch lookup value from a database or a server side cache.
        /// </summary>
        /// <remarks>If required, filter out the returned lookup values for the current user.</remarks>
        protected abstract Task<List<TLookup>> DoFetchAsync<TLookup, TEntity>(CancellationToken ct)
            where TLookup : LookupBase<TEntity>
            where TEntity : class, IDomainEntity<TEntity>;


        /// <summary>
        /// Authorizes a lookup request: the requester must be a use case that declares the lookup
        /// (<see cref="UseCaseAttribute.LookupTypes"/>), and the scope's identity must be allowed to invoke it.
        /// </summary>
        /// <param name="requesterType">the type of the use case requesting the lookup</param>
        /// <exception cref="NotAuthenticatedException">not logged in, or the session has ended</exception>
        /// <exception cref="NotAuthorizedException">the identity may not invoke the requesting use case</exception>
        private async Task AuthorizeAsync<TLookup>(Type requesterType, CancellationToken ct)
        {
            if (!typeof(IAuthorizedUseCase).IsAssignableFrom(requesterType))
                throw new ArgumentException($"Only use cases that implement IAuthorizedUseCase " +
                    $"can request lookup values while {requesterType.FullName} does not.");

            UseCaseAttribute attribute = null;
            foreach (var implementedInterface in requesterType.GetInterfaces())
            {
                attribute = implementedInterface.GetCustomAttributes<UseCaseAttribute>().FirstOrDefault();
                if (null != attribute) break;
            }
            if (null == attribute) throw new ArgumentException(
                $"Only use cases that that are marked with a UseCaseAttribute " +
                $"can request lookup values while {requesterType.FullName} is not.");
            if (null == attribute.LookupTypes || Array.IndexOf(attribute.LookupTypes, typeof(TLookup)) < 0)
                throw new ArgumentException($"Use case {requesterType.FullName} " +
                    $"does not require a lookup of type {typeof(TLookup).FullName}.");

            if (!typeof(IAuthorizedUseCase).IsAssignableFrom(requesterType)) return;

            var authorizer = AuthorizationProvider.GetAuthorizer(requesterType);
            await _authenticationStateProvider.AuthorizeAsync(authorizer, ct)
                .ConfigureAwait(false);
        }
    }
}

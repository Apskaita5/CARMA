using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Caching;
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
        private readonly IExceptionFactory _exceptions;
        private readonly IAuthenticationStateProvider _authenticationStateProvider;
        private readonly IClientCache _clientCache;


        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="authenticationStateProvider">the scope's identity</param>
        /// <param name="authorizationProvider">the authorization rules of the use cases</param>
        /// <param name="dataPortal">the client data portal of the scope</param>
        /// <param name="exceptions">the app's factory of localized exceptions</param>
        /// <param name="clientCache">the scope's client cache, for the lookups fetched from a remote server</param>
        /// <param name="logger">a logger; null for none</param>
        protected GenericLookupService(IAuthenticationStateProvider authenticationStateProvider,
            IAuthorizationProvider authorizationProvider, IClientDataPortal dataPortal,
            IExceptionFactory exceptions, IClientCache clientCache, ILogger logger = null)
        {
            _exceptions = exceptions ?? throw new ArgumentNullException(nameof(exceptions));
            _authenticationStateProvider = authenticationStateProvider
                ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            AuthorizationProvider = authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _clientCache = clientCache ?? throw new ArgumentNullException(nameof(clientCache));
            Logger = logger;
        }


        /// <summary>Gets the authorization rules of the use cases.</summary>
        protected IAuthorizationProvider AuthorizationProvider { get; }

        /// <summary>Gets the logger; null for none.</summary>
        protected ILogger Logger { get; }


        /// <inheritdoc />
        public async Task<List<TLookup>> FetchAsync<TLookup, TEntity>(Type requesterType,
            CancellationToken ct = default)
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
                    // kept for the scope: the server may filter the list for the scope's identity
                    if (!_clientCache.TryGet(typeof(TLookup), out result))
                    {
                        result = await _dataPortal.FetchAsync<Type, List<TLookup>>(GetType(), requesterType,
                            new[] { typeof(TLookup), typeof(TEntity) }, ct)
                            .ConfigureAwait(false);

                        _clientCache.Set(typeof(TLookup), result, ClientCacheLifetimeAttribute.GetLifetime(typeof(TLookup))
                            ?? _clientCache.DefaultLifetime, typeof(TEntity));
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
                var toThrow = UseCaseBoundary.Handle(e, ct, Logger, _exceptions,
                    GetType(), nameof(FetchAsync));
                if (ReferenceEquals(toThrow, e)) throw;
                throw toThrow;
            }

            Logger?.LogMethodExit(this.GetType(), nameof(FetchAsync));

            return result;
        }


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

            var authorizer = AuthorizationProvider.GetAuthorizer(requesterType);
            await _authenticationStateProvider.AuthorizeAsync(authorizer, ct)
                .ConfigureAwait(false);
        }
    }
}

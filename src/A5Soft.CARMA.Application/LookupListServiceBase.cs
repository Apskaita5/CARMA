using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Base class for service that fetches a list of lookup values). 
    /// </summary>
    /// <typeparam name="TLookup">a type of the managed lookup value</typeparam>
    /// <remarks>Lookups (e.g. list of persons to be displayed in dropdowns) contain
    /// sensitive personal and/or business data. However there is no reasonable way
    /// to manage access authorization as the need of a particular lookup
    /// is defined by business entities that the user has access to.
    /// I.e. if a user has write access to invoices he needs a lookup for persons.
    /// Therefore we need a service that is only accessible by use cases
    /// which transfers authorization responsibility to them.
    /// E.g. save invoice use case shall be responsible for providing all the necessary lookups
    /// for a user in order to edit an invoice.</remarks>
    public abstract class LookupListServiceBase<TLookup> where TLookup : class
    {
        protected readonly ILogger _logger;
        private readonly IClientDataPortal _dataPortal;
        private readonly IAuthenticationStateProvider _authenticationStateProvider;
        protected bool _allowAnonymousUseCases = false;


        /// <inheritdoc cref="IAuthorizedUseCase.GetIdentityAsync" />
        public Task<ClaimsIdentity> GetIdentityAsync()
        {
            return _authenticationStateProvider.GetIdentityAsync();
        }

        /// <summary>
        /// Gets an authorization provider.
        /// </summary>
        protected IAuthorizationProvider AuthorizationProvider { get; }


        protected LookupListServiceBase(IAuthenticationStateProvider authenticationStateProvider,
            IAuthorizationProvider authorizationProvider, IClientDataPortal dataPortal, ILogger logger)
        {
            _authenticationStateProvider = authenticationStateProvider
                ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            AuthorizationProvider = authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Gets a lookup value.
        /// </summary>
        /// <param name="requesterType">a type of the use case that requests the lookup</param>
        public async Task<List<TLookup>> FetchAsync(Type requesterType)
        {
            if (null == requesterType) throw new ArgumentNullException(nameof(requesterType));

            _logger?.LogMethodEntry(this.GetType(), nameof(FetchAsync), requesterType);

            List<TLookup> result;

            if (_dataPortal.IsRemote)
            {
                try
                {
                    result = GetValueFromLocalCache();

                    if (!result.IsNull())
                    {
                        _logger?.LogMethodExit(this.GetType(), nameof(FetchAsync), result);

                        return result;
                    }

                    result = (await _dataPortal.FetchAsync<Type, List<TLookup>>(this.GetType(),
                        requesterType, await GetIdentityAsync())).Result;

                    SetLocalCacheValue(result);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(FetchAsync));

                return result;
            }

            try
            {
                await AuthorizeAsync(requesterType);

                result = await DoFetchAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(FetchAsync));

            return result;
        }

        /// <summary>
        /// Implement this method to get a requested value from a local cache (if used/configured).
        /// </summary>
        /// <remarks>Do not configure local cache for shared environments even if a remote data portal is used.
        /// The data portal will return only lookups for the user, which will render the
        /// shared cache invalid. Data portal cannot return lookup values that are not meant for the user
        /// as it will compromise its security.</remarks>
        protected abstract List<TLookup> GetValueFromLocalCache();

        /// <summary>
        /// Implement this method to add fetched lookup value to a local cache (if used/configured).
        /// </summary>
        /// <remarks>Only invoked if a remote data portal is used.
        /// Do not configure local cache for shared environments even if a remote data portal is used.
        /// The data portal might return only lookups for the user, which will render the
        /// shared cache invalid. Data portal cannot return lookup values that are not meant for the user
        /// as it will compromise its security.</remarks>
        protected abstract void SetLocalCacheValue(List<TLookup> value);

        /// <summary>
        /// Implement this method to fetch lookup value from a database or a server side cache.
        /// </summary>
        /// <remarks>If required, filter out the returned lookup values for the <see cref="User"/>.</remarks>
        protected abstract Task<List<TLookup>> DoFetchAsync();


        private async Task AuthorizeAsync(Type requesterType)
        {
            if (!_allowAnonymousUseCases && !typeof(IAuthorizedUseCase).IsAssignableFrom(requesterType))
                throw new ArgumentException($"Only use cases that implement IAuthorizedUseCase " +
                    $"can request lookup values while {requesterType.FullName} does not.");
            if (!typeof(IUseCase).IsAssignableFrom(requesterType))
                throw new ArgumentException($"Only use cases can request lookup values while " +
                    $"{requesterType.FullName} is not.");

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
            if (!authorizer.IsAuthorized(await GetIdentityAsync())) throw new SecurityException(
                $"User is not authorized to invoke the requesting use case ({requesterType.FullName}).");
        }
    }
}

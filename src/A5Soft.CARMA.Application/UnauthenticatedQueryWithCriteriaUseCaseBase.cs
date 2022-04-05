using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for an unauthenticated query operation (that gets a query result
    /// for an unauthenticated user using query criteria provided).
    /// E.g. login use case returns ClaimsIdentity.
    /// </summary>
    /// <typeparam name="TResult">a type of the query result (must be binary serializable)</typeparam>
    /// <typeparam name="TCriteria">a type of the query parameter (must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    public abstract class UnauthenticatedQueryWithCriteriaUseCaseBase<TResult, TCriteria> : RemoteUseCaseBase
    {
        protected readonly IAuthenticationStateProvider _authenticationStateProvider;


        /// <inheritdoc />
        protected UnauthenticatedQueryWithCriteriaUseCaseBase(IClientDataPortal dataPortal,
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider,
            IAuthenticationStateProvider authenticationStateProvider, ILogger logger) 
            : base(dataPortal, validationProvider, metadataProvider, logger)
        {
            if (!typeof(TResult).IsSerializable) throw new InvalidOperationException(
                $"Query result must be (binary) serializable while type {typeof(TResult).FullName} is not.");

            _authenticationStateProvider = authenticationStateProvider ??
                throw new ArgumentNullException(nameof(authenticationStateProvider));
        }


        /// <summary>
        /// Whether to use a default logging for method entry and exit.
        /// Disabled by default as the use case is commonly used for authentication
        /// while sensitive data shall not be logged.
        /// Does not affect exception logging.
        /// </summary>
        protected virtual bool UseDefaultLogging { get; } = false;


        /// <summary>
        /// Gets a query result using query criteria provided.
        /// </summary>
        /// <param name="criteria">a criteria for the query</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a query result</returns>
        public async Task<TResult> InvokeAsync(TCriteria criteria, CancellationToken ct = default)
        {
            if (UseDefaultLogging) Logger.LogMethodEntry(this.GetType(), nameof(InvokeAsync), criteria);

            TResult result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(criteria, ct);

                    var dpResult = await DataPortal.FetchUnauthenticatedAsync<TCriteria, TResult>(
                        this.GetType(), criteria, ct);

                    result = dpResult.Result;
                    if (result is ITrackState stateful) stateful.SetValidationEngine(ValidationProvider);

                    if (null != dpResult.Identity) 
                        await _authenticationStateProvider.NotifyIdentityChangedAsync(dpResult.Identity);

                    await AfterDataPortalAsync(criteria, result, ct);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                if (UseDefaultLogging) Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return result;
            }

            try
            {
                result = await QueryIntAsync(criteria, ct);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));

            return result;
        }


        /// <summary>
        /// Implement this method to fetch a query result.
        /// </summary>
        /// <param name="criteria">a criteria for the query</param>
        /// <returns>a query result</returns>
        /// <remarks>The criteria param is NOT guaranteed to be not null (as it could be a valid option).
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TResult> QueryIntAsync(TCriteria criteria, CancellationToken ct);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(TCriteria criteria, CancellationToken ct)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. 
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TCriteria criteria, TResult result,
            CancellationToken ct)
            => Task.CompletedTask;


        /// <summary>
        /// Gets metadata for the entity fetched.
        /// Returns null if the entity is not a class or an interface.
        /// </summary>
        public IEntityMetadata GetMetadata()
        {
            var resultType = typeof(TResult);
            if (!resultType.IsInterface && !resultType.IsClass) return null;
            if (resultType == typeof(string)) return null;
            return MetadataProvider.GetEntityMetadata<TResult>();
        }

        /// <summary>
        /// Gets metadata for the query criteria.
        /// Returns null if the criteria is not a class or an interface.
        /// </summary>
        public IEntityMetadata GetCriteriaMetadata()
        {
            var criteriaType = typeof(TCriteria);
            if (!criteriaType.IsInterface && !criteriaType.IsClass) return null;
            if (criteriaType == typeof(string)) return null;
            return MetadataProvider.GetEntityMetadata(criteriaType);
        }

        /// <summary>
        /// Validates a criteria (as a POCO object) and returns a broken rules collection
        /// that can be used to determine whether the criteria is valid and what are the
        /// broken rules (if invalid).
        /// </summary>
        /// <param name="criteria">criteria to validate</param>
        /// <remarks>Override this method in order to implement custom validation
        /// or disable validation by returning a new (empty) <see cref="BrokenRulesCollection"/>.</remarks>
        public virtual BrokenRulesCollection Validate(TCriteria criteria)
        {
            return ValidationProvider.ValidatePoco(criteria);
        }

        /// <inheritdoc cref="IUseCaseMetadata.GetButtonTitle"/>
        public string GetButtonTitle()
        {
            return MetadataProvider.GetUseCaseMetadata(this.GetType()).GetButtonTitle();
        }
    }
}

﻿using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain;
using System.Threading;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a query operation (that gets a query result using query criteria provided).
    /// </summary>
    /// <typeparam name="TResult">a type of the query result (must be binary serializable)</typeparam>
    /// <typeparam name="TCriteria">a type of the query parameter (must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    public abstract class QueryWithCriteriaUseCaseBase<TResult, TCriteria> : AuthorizedUseCaseBase
        where TResult : class
    {
        /// <inheritdoc />
        protected QueryWithCriteriaUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger)
        {
            if (!typeof(TResult).IsSerializable) throw new InvalidOperationException(
                $"Query result must be (binary) serializable while type {typeof(TResult).FullName} is not.");
        }


        /// <summary>
        /// Gets a query result using query criteria provided.
        /// </summary>
        /// <param name="criteria">a criteria for the query</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a query result</returns>
        public async Task<TResult> QueryAsync(TCriteria criteria, CancellationToken ct = default)
        {
            Logger.LogMethodEntry(this.GetType(), nameof(QueryAsync), criteria);

            TResult result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(criteria, ct);
                    result = (await DataPortal.FetchAsync<TCriteria, TResult>(
                        this.GetType(), criteria, await GetIdentityAsync(), ct)).Result;
                    await AfterDataPortalAsync(criteria, result, ct);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(QueryAsync));

                return result;
            }

            await CanInvokeAsync(true);
             
            try
            {
                result = await DoQueryAsync(criteria, ct);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(QueryAsync));

            return result;
        }

        /// <summary>
        /// Implement this method to fetch a query result.
        /// </summary>
        /// <param name="criteria">a criteria for the query</param>
        /// <returns>a query result</returns>
        /// <remarks>At this stage user has already been authorized.
        /// The criteria param is NOT guaranteed to be not null (as it could be a valid option).
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TResult> DoQueryAsync(TCriteria criteria, CancellationToken ct);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(TCriteria criteria, CancellationToken ct)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TCriteria criteria, TResult result, CancellationToken ct)
            => Task.CompletedTask;


        /// <summary>
        /// Gets metadata for the entity fetched.
        /// </summary>
        public IEntityMetadata GetMetadata()
            => MetadataProvider.GetEntityMetadata<TResult>();

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

        /// <inheritdoc cref="IUseCaseMetadata.GetMenuTitle"/>
        public string GetMenuTitle()
        {
            return MetadataProvider.GetUseCaseMetadata(this.GetType()).GetMenuTitle();
        }
    }
}

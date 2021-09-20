using System;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Creates a new entity based on create criteria and returns either the created entity or its id.
    /// </summary>
    /// <typeparam name="TCriteria">type of the create criteria (must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    /// <typeparam name="TResult">type of the result returned (either the created entity or its id)</typeparam>
    public abstract class CreateUseCaseBase<TCriteria, TResult> : AuthorizedUseCaseBase
    {
        /// <inheritdoc />
        protected CreateUseCaseBase(IAuthenticationStateProvider authenticationStateProvider,
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal,
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger)
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger)
        {
            if (!typeof(TResult).IsSerializable) throw new InvalidOperationException(
                $"Result must be (binary) serializable while type {typeof(TResult).FullName} is not.");
        }


        /// <summary>
        /// Executes the command using the parameter provided.
        /// </summary>
        /// <param name="criteria">a parameter for the command</param>
        public async Task<TResult> InvokeAsync(TCriteria criteria)
        {
            Logger.LogMethodEntry(this.GetType(), nameof(InvokeAsync), criteria);

            TResult result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(criteria);
                    result = await DataPortal.FetchAsync<TCriteria, TResult>(this.GetType(), 
                        criteria, await GetIdentityAsync());
                    await AfterDataPortalAsync(criteria);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return result;
            }

            await CanInvokeAsync(true);

            try
            {
                result = await ExecuteAsync(criteria);
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
        /// Implement this method to execute the command.
        /// </summary>
        /// <param name="criteria">a create criteria</param>
        /// <remarks>At this stage user has already been authorized.
        /// The parameter is NOT guaranteed to be not null (as it could be a valid option).
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TResult> ExecuteAsync(TCriteria criteria);


        /// <summary>
        /// Gets metadata for the create criteria.
        /// </summary>
        public IEntityMetadata GetMetadata()
            => MetadataProvider.GetEntityMetadata<TCriteria>();


        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(TCriteria criteria)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TCriteria criteria)
            => Task.CompletedTask;

    }
}

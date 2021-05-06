using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a command operation (that executes a command using the parameters provided without a result).
    /// </summary>
    /// <typeparam name="TParameter">a type of the command parameter(must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    public abstract class CommandWithParameterUseCaseBase<TParameter> : AuthorizedUseCaseBase
    {
        /// <inheritdoc />
        protected CommandWithParameterUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger) { }


        /// <summary>
        /// Executes the command using the parameter provided.
        /// </summary>
        /// <param name="parameter">a parameter for the command</param>
        public async Task InvokeAsync(TParameter parameter)
        {
            Logger.LogMethodEntry(this.GetType(), nameof(InvokeAsync), parameter);

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(parameter);
                    await DataPortal.InvokeAsync(this.GetType(), parameter, await GetIdentityAsync());
                    await AfterDataPortalAsync(parameter);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return;
            }

            if (Authorizer.AuthorizationImplementedForParam<TParameter>())
                Authorizer.IsAuthorized(await GetIdentityAsync(), parameter, true);
            else await CanInvokeAsync(true);

            try
            {
                await ExecuteAsync(parameter);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));
        }

        /// <summary>
        /// Implement this method to execute the command.
        /// </summary>
        /// <param name="parameter">a parameter for the command</param>
        /// <remarks>At this stage user has already been authorized.
        /// The parameter is NOT guaranteed to be not null (as it could be a valid option).
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task ExecuteAsync(TParameter parameter);


        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(TParameter parameter)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TParameter parameter)
            => Task.CompletedTask;


        /// <summary>
        /// Gets metadata for the command parameter.
        /// Returns null if the parameter is not a class or an interface.
        /// </summary>
        public IEntityMetadata GetParameterMetadata()
        {
            var criteriaType = typeof(TParameter);
            if (!criteriaType.IsInterface && !criteriaType.IsClass) return null;
            if (criteriaType == typeof(string)) return null;
            return MetadataProvider.GetEntityMetadata(criteriaType);
        }

        /// <summary>
        /// Validates a parameter (as a POCO object) and returns a broken rules collection
        /// that can be used to determine whether the parameter is valid and what are the
        /// broken rules (if invalid).
        /// </summary>
        /// <param name="parameter">parameter to validate</param>
        /// <remarks>Override this method in order to implement custom validation
        /// or disable validation by returning a new (empty) <see cref="BrokenRulesCollection"/>.</remarks>
        public virtual BrokenRulesCollection Validate(TParameter parameter)
        {
            return ValidationProvider.ValidatePoco(parameter);
        }

    }
}

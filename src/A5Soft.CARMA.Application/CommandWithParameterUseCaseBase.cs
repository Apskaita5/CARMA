using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using System;
using System.Security.Claims;
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
        protected readonly ILogger _logger;
        private readonly IClientDataPortal _dataPortal;


        /// <inheritdoc />
        protected CommandWithParameterUseCaseBase(ILogger logger, IClientDataPortal dataPortal,
            IAuthorizationProvider authorizationProvider, ClaimsIdentity userIdentity) : 
            base(authorizationProvider, userIdentity)
        {
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Executes the command using the parameter provided.
        /// </summary>
        /// <param name="parameter">a parameter for the command</param>
        public async Task InvokeAsync(TParameter parameter)
        {
            _logger?.LogMethodEntry(this.GetType(), nameof(InvokeAsync), parameter);

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(parameter);
                    await _dataPortal.InvokeAsync(this.GetType().GetRemoteServiceInterfaceType(), parameter, User);
                    await AfterDataPortalAsync(parameter);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return;
            }

            if (Authorizer.AuthorizationImplementedForParam<TParameter>())
                Authorizer.IsAuthorized(User, parameter, true);
            else CanInvoke(true);

            try
            {
                await ExecuteAsync(parameter);
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync));
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

    }
}

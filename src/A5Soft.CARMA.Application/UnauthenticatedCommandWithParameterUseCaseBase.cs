using System;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.DataPortal;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for an unauthenticated command operation (that executes a command
    /// by an unauthenticated user using the parameters provided without a result).
    /// E.g. reset password, signup etc.
    /// </summary>
    /// <typeparam name="TParameter">a type of the command parameter(must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    public abstract class UnauthenticatedCommandWithParameterUseCaseBase<TParameter>
    {
        protected readonly ILogger _logger;
        private readonly IClientDataPortal _dataPortal;

        protected UnauthenticatedCommandWithParameterUseCaseBase(ILogger logger, 
            IClientDataPortal dataPortal)
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
                    await _dataPortal.InvokeAsync(this.GetType().GetRemoteServiceInterfaceType(), parameter);
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
        /// <remarks>The parameter is NOT guaranteed to be not null (as it could be a valid option).
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
        /// a successful remote invocation. 
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TParameter parameter)
            => Task.CompletedTask;

    }
}

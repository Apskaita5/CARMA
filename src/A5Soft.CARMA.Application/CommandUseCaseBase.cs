using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a parameterless command operation (that executes a command without any parameters).
    /// </summary>
    public abstract class CommandUseCaseBase : AuthorizedUseCaseBase
    {
        private readonly IClientDataPortal _dataPortal;
        protected readonly ILogger _logger;


        /// <inheritdoc />
        protected CommandUseCaseBase(ILogger logger, IClientDataPortal dataPortal, 
            IAuthorizationProvider authorizationProvider, ClaimsIdentity userIdentity) 
            : base(authorizationProvider, userIdentity)
        {
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Executes the command.
        /// </summary>
        public async Task InvokeAsync()
        {
            _logger?.LogMethodEntry(this.GetType(), nameof(InvokeAsync));

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync();
                    await _dataPortal.InvokeAsync(this.GetType().GetRemoteServiceInterfaceType(), User);
                    await AfterDataPortalAsync();
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }
                
                _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return;
            }

            CanInvoke(true);

            try
            {
                await ExecuteAsync();
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
        /// <remarks>At this stage user has already been authorized.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task ExecuteAsync();

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync()
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync()
            => Task.CompletedTask;

    }
}

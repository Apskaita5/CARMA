using A5Soft.CARMA.Application.Authorization.Identity;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Commands
{
    /// <summary>
    /// A command without a parameter or a result: no exception means done.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class CommandUseCaseBase<TAppContext, TIdentity> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected CommandUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Executes the command.
        /// </summary>
        public Task ExecuteAsync() => RunAsync(dp => dp.InvokeAsync(GetType()), DoExecuteAsync);


        /// <summary>
        /// Executes the command.
        /// </summary>
        protected abstract Task DoExecuteAsync();
    }
}

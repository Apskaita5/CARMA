using A5Soft.CARMA.Application.Authorization.Identity;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Commands
{
    /// <summary>
    /// A command with a parameter and a result, e.g. posting an invoice and returning its updated list row, so
    /// that the client need not fetch the list again.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TParameter">the type of the parameter</typeparam>
    /// <typeparam name="TResult">the type of the result</typeparam>
    public abstract class CommandWithResultUseCaseBase<TAppContext, TIdentity, TParameter, TResult> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected CommandWithResultUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">the parameter</param>
        /// <returns>the result</returns>
        public Task<TResult> ExecuteAsync(TParameter parameter)
        {
            if (null == parameter) throw new ArgumentNullException(nameof(parameter));

            return RunAsync(dp => dp.FetchAsync<TParameter, TResult>(GetType(), parameter),
                () => DoExecuteAsync(parameter), parameter);
        }


        /// <summary>
        /// Executes the command. The parameter comes from the client: do not trust it.
        /// </summary>
        /// <param name="parameter">the parameter</param>
        protected abstract Task<TResult> DoExecuteAsync(TParameter parameter);
    }
}

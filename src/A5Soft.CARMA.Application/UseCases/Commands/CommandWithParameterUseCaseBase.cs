using A5Soft.CARMA.Application.Authorization.Identity;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Commands
{
    /// <summary>
    /// A command with a parameter and no result: no exception means done.
    /// </summary>
    /// <remarks>A parameter the user never sees (e.g. the id of the entity to post) is a POCO; one the user edits
    /// is a domain object, which the app merges into a trusted instance.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TParameter">the type of the parameter</typeparam>
    public abstract class CommandWithParameterUseCaseBase<TAppContext, TIdentity, TParameter>
        : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected CommandWithParameterUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">the parameter</param>
        public Task ExecuteAsync(TParameter parameter)
        {
            if (null == parameter) throw new ArgumentNullException(nameof(parameter));

            return RunAsync(dp => dp.InvokeAsync(GetType(), parameter),
                () => DoExecuteAsync(parameter), parameter);
        }


        /// <summary>
        /// Executes the command. The parameter comes from the client: do not trust it.
        /// </summary>
        /// <param name="parameter">the parameter</param>
        protected abstract Task DoExecuteAsync(TParameter parameter);
    }
}

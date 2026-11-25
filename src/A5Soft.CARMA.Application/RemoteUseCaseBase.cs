using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.Retry;
using A5Soft.CARMA.Domain;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base class for use cases that run through the data portal when the scope's endpoint is remote, and locally
    /// otherwise. Holds the only data portal calls of the use cases.
    /// </summary>
    /// <remarks>
    /// <para>Two ways to run the remote method:</para>
    /// <list type="bullet">
    /// <item><see cref="RunAsync{TResult}"/> for a method that changes state: it cannot be cancelled, as a cancelled
    /// call does not prove that the change was not made.</item>
    /// <item><see cref="RunReadOnlyAsync{TResult}"/> for a method that does not: it takes a
    /// <see cref="CancellationToken"/> (as the last parameter of the remote method too).</item>
    /// </list>
    /// <para>Neither retries. When the data portal server cannot be reached, the client data portal throws a
    /// <see cref="BusinessException"/> (a <see cref="TransiantException"/> as an inner exception) with a message for the user,
    /// who repeats the action later; a background service retries the use case itself, if it should.</para>
    /// <para>A local use case (its interface marked <see cref="LocalUseCaseAttribute"/>) always runs locally, also
    /// where the scope's endpoint is remote.</para>
    /// </remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    public abstract class RemoteUseCaseBase<TAppContext> : UseCaseBase<TAppContext>
        where TAppContext : class, IApplicationContext
    {
        private static readonly ConcurrentDictionary<Type, bool> LocalUseCases = new ConcurrentDictionary<Type, bool>();

        /// <inheritdoc />
        protected RemoteUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Gets a value indicating whether the use case is local: its interface is marked
        /// <see cref="LocalUseCaseAttribute"/>, so it never goes through the data portal.
        /// </summary>
        protected bool IsLocal => LocalUseCases.GetOrAdd(GetType(), type => type.GetInterfaces()
            .Any(i => i.IsDefined(typeof(LocalUseCaseAttribute), false)));


        /// <summary>
        /// Runs a remote method that changes state: through the data portal when the scope's endpoint is remote,
        /// otherwise locally after <see cref="BeforeLocalRunAsync"/>. Logs the entry, the exit and a failure.
        /// </summary>
        /// <typeparam name="TResult">the type of the result</typeparam>
        /// <param name="remote">the data portal call</param>
        /// <param name="local">the local implementation</param>
        /// <param name="loggedArgument">the argument to log; never credentials, codes or file contents</param>
        /// <param name="method">the remote method's name, filled in by the compiler</param>
        /// <returns>the result of the remote method</returns>
        protected Task<TResult> RunAsync<TResult>(Func<IClientDataPortal, Task<TResult>> remote,
            Func<Task<TResult>> local, object loggedArgument = null, [CallerMemberName] string method = null)
        {
            if (null == remote) throw new ArgumentNullException(nameof(remote));
            if (null == local) throw new ArgumentNullException(nameof(local));

            return RunCoreAsync((dp, ct) => remote(dp), ct => local(),
                CancellationToken.None, loggedArgument, method);
        }

        /// <summary>
        /// Runs a remote method that changes state and returns nothing; see <see cref="RunAsync{TResult}"/>.
        /// </summary>
        /// <param name="remote">the data portal call</param>
        /// <param name="local">the local implementation</param>
        /// <param name="loggedArgument">the argument to log; never credentials, codes or file contents</param>
        /// <param name="method">the remote method's name, filled in by the compiler</param>
        protected Task RunAsync(Func<IClientDataPortal, Task> remote, Func<Task> local,
            object loggedArgument = null, [CallerMemberName] string method = null)
        {
            if (null == remote) throw new ArgumentNullException(nameof(remote));
            if (null == local) throw new ArgumentNullException(nameof(local));

            return RunCoreAsync<object>(
                async (dp, ct) =>
                {
                    await remote(dp).ConfigureAwait(false);
                    return null;
                },
                async ct =>
                {
                    await local().ConfigureAwait(false);
                    return null;
                }, CancellationToken.None, loggedArgument, method);
        }

        /// <summary>
        /// Runs a remote method that does not change state, so that it can be cancelled. Otherwise as
        /// <see cref="RunAsync{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">the type of the result</typeparam>
        /// <param name="remote">the data portal call; pass it the token, and the remote method must take one</param>
        /// <param name="local">the local implementation</param>
        /// <param name="cancellationToken">a token to cancel the call</param>
        /// <param name="loggedArgument">the argument to log; never credentials, codes or file contents</param>
        /// <param name="method">the remote method's name, filled in by the compiler</param>
        /// <returns>the result of the remote method</returns>
        protected Task<TResult> RunReadOnlyAsync<TResult>(Func<IClientDataPortal, CancellationToken, Task<TResult>> remote,
            Func<CancellationToken, Task<TResult>> local, CancellationToken cancellationToken,
            object loggedArgument = null, [CallerMemberName] string method = null)
        {
            if (null == remote) throw new ArgumentNullException(nameof(remote));
            if (null == local) throw new ArgumentNullException(nameof(local));

            return RunCoreAsync(remote, local, cancellationToken, loggedArgument, method);
        }

        /// <summary>
        /// Runs before the local implementation: <see cref="AuthorizedUseCaseBase{TAppContext,TIdentity}"/>
        /// authorizes here.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        protected virtual Task BeforeLocalRunAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Logs a failure by its kind: a security issue; a warning for a business failure or an outage (a
        /// <see cref="TransiantException"/>, e.g. <see cref="DataPortalUnavailableException"/>); a cancellation; else
        /// an error.
        /// </summary>
        /// <param name="exception">the failure</param>
        protected virtual void LogFailure(Exception exception)
        {
            switch (exception)
            {
                case NotAuthorizedExceptionBase _:
                    Context.Logger.LogSecurityIssue(exception);
                    break;
                case NotAuthenticatedException _:
                case BusinessException _:
                case TransiantException _:
                    Context.Logger.LogDebug(exception);
                    break;
                case OperationCanceledException _:
                    Context.Logger.LogDebug($"{GetType().Name}: cancelled.");
                    break;
                default:
                    Context.Logger.LogError(exception);
                    break;
            }
        }


        private async Task<TResult> RunCoreAsync<TResult>(Func<IClientDataPortal, CancellationToken, Task<TResult>> remote,
            Func<CancellationToken, Task<TResult>> local, CancellationToken cancellationToken,
            object loggedArgument, string method)
        {
            Context.Logger.LogMethodEntry(GetType(), method, loggedArgument);

            TResult result;
            try
            {
                if (Context.DataPortal.IsRemote && !IsLocal)
                {
                    result = await remote(Context.DataPortal, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await BeforeLocalRunAsync(cancellationToken).ConfigureAwait(false);

                    result = await local(cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                LogFailure(e);
                throw;
            }

            Context.Logger.LogMethodExit(GetType(), method, result);
            return result;
        }
    }
}

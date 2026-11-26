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
    /// <para>Neither retries. Both branches are the use case boundary (<see cref="ExceptionExtensions"/>): a
    /// cancellation is rethrown as it is; an authorization failure is logged as a security issue; any other business
    /// exception, or a <see cref="NotAuthenticatedException"/>, is logged at debug level; a
    /// <see cref="TransientException"/> (e.g. the client data portal's
    /// <see cref="DataPortalUnavailableException"/>) is logged as a warning and thrown as the app's business exception
    /// (<see cref="IExceptionFactory.ServiceUnavailable"/>), which holds it; anything else is logged as an
    /// error with a correlation id. Each exception is logged once: not again by an outer use case, nor on the client
    /// when the server has logged it.</para>
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
        /// Runs a remote method that does not change state, keeping its result in the scope's client cache
        /// (<see cref="IApplicationContext.ClientCache"/>): the data portal is called only if the cache holds no
        /// current value for the key. A local run is never cached. Otherwise as
        /// <see cref="RunReadOnlyAsync{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">the type of the result</typeparam>
        /// <param name="cacheKey">the type to keep the result under: the result type</param>
        /// <param name="lifetime">how long to keep it; zero for not at all</param>
        /// <param name="entityType">the entity type the result depends on, if any (dropped with it)</param>
        /// <param name="remote">the data portal call; pass it the token, and the remote method must take one</param>
        /// <param name="local">the local implementation</param>
        /// <param name="cancellationToken">a token to cancel the call</param>
        /// <param name="loggedArgument">the argument to log; never credentials, codes or file contents</param>
        /// <param name="method">the remote method's name, filled in by the compiler</param>
        /// <returns>the result of the remote method; a cached one is shared, never change it</returns>
        protected Task<TResult> RunCachedReadOnlyAsync<TResult>(Type cacheKey, TimeSpan lifetime, Type entityType,
            Func<IClientDataPortal, CancellationToken, Task<TResult>> remote,
            Func<CancellationToken, Task<TResult>> local, CancellationToken cancellationToken,
            object loggedArgument = null, [CallerMemberName] string method = null)
        {
            if (null == cacheKey) throw new ArgumentNullException(nameof(cacheKey));
            if (null == remote) throw new ArgumentNullException(nameof(remote));
            if (null == local) throw new ArgumentNullException(nameof(local));

            if (lifetime <= TimeSpan.Zero) return RunCoreAsync(remote, local, cancellationToken, loggedArgument, method);

            return RunCoreAsync(async (dp, ct) =>
            {
                if (Context.ClientCache.TryGet(cacheKey, out TResult cached)) return cached;

                var result = await remote(dp, ct).ConfigureAwait(false);

                Context.ClientCache.Set(cacheKey, result, lifetime, entityType);

                return result;
            }, local, cancellationToken, loggedArgument, method);
        }

        /// <summary>
        /// Runs before the local implementation: <see cref="AuthorizedUseCaseBase{TAppContext,TIdentity}"/>
        /// authorizes here.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        protected virtual Task BeforeLocalRunAsync(CancellationToken cancellationToken) => Task.CompletedTask;

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
                var toThrow = UseCaseBoundary.Handle(e, cancellationToken, Context.Logger,
                    Context.ExceptionFactory, GetType(), method);
                if (ReferenceEquals(toThrow, e)) throw;
                throw toThrow;
            }

            Context.Logger.LogMethodExit(GetType(), method, result);
            return result;
        }
    }
}

using A5Soft.CARMA.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The client side of the data portal, as a use case sees it: invokes the remote method of a use case
    /// on the server and gets its result back. Implemented by <see cref="ClientDataPortal"/>.
    /// </summary>
    /// <remarks>
    /// <para>A use case calls it only when <see cref="IsRemote"/> is true, and only from an app-level use
    /// case base class, never from a concrete use case: the base class runs the use case locally
    /// otherwise.</para>
    /// <para>Every method takes the use case type (its interface or implementation) and the arguments of
    /// its remote method, in signature order. The caller's identity must hold the token claim
    /// (<see cref="IDataPortalClientOptions.TokenClaimType"/>); null makes an anonymous call, which the
    /// server allows for unauthenticated use cases only.</para>
    /// <para>Every method returns the caller's refreshed identity, or null if it has not changed; the
    /// caller replaces its identity with a non-null one.</para>
    /// <para>Parameters: a domain object travels as its DTO and is rebuilt on the server; any
    /// <see cref="FileContent"/> in the arguments, however deeply nested, is uploaded as a file. The caller
    /// keeps owning the files.</para>
    /// <para>Results: a domain object is rebuilt from its DTO on the client. A <see cref="FileContent"/>
    /// result is a download, streamed as it arrives: the caller must dispose it. A file nested in a result
    /// is always null.</para>
    /// <para>Failures are thrown as the application exceptions the <see cref="IRemoteErrorMapper"/> maps
    /// them to, or as <see cref="RemoteCallException"/>; see <see cref="RemoteErrorKind"/>.</para>
    /// </remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IClientDataPortal
    {
        /// <summary>
        /// Gets a value indicating whether use cases run remotely, i.e. whether a remote
        /// <see cref="IDataPortalProxy"/> is registered. When false, use cases run locally and the data
        /// portal must not be called.
        /// </summary>
        bool IsRemote { get; }


        /// <summary>Invokes a remote method without parameters that returns a value.</summary>
        /// <typeparam name="TResult">the return type of the remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the result and the refreshed identity (null if unchanged)</returns>
        Task<(TResult Result, ClaimsIdentity Identity)> FetchAsync<TResult>(Type useCaseType,
            ClaimsIdentity identity);

        /// <summary>Invokes a remote method with one parameter that returns a value.</summary>
        /// <typeparam name="TArg">the type of the parameter</typeparam>
        /// <typeparam name="TResult">the return type of the remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg">the argument</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the result and the refreshed identity (null if unchanged)</returns>
        Task<(TResult Result, ClaimsIdentity Identity)> FetchAsync<TArg, TResult>(Type useCaseType,
            TArg arg, ClaimsIdentity identity);

        /// <summary>
        /// Invokes a generic remote method with one parameter that returns a value, closed over the generic
        /// arguments given, e.g. <c>FetchAsync&lt;TLookup, TEntity&gt;(Type requester)</c> of a lookup service.
        /// </summary>
        /// <typeparam name="TArg">the type of the parameter</typeparam>
        /// <typeparam name="TResult">the return type of the closed remote method</typeparam>
        /// <param name="useCaseType">the use case (remote service) interface or implementation type</param>
        /// <param name="arg">the argument</param>
        /// <param name="genericArgs">the generic arguments of the remote method. Each must be a trusted wire type
        /// (<see cref="RemoteUseCaseRegistry.TrustedWireTypes"/>): a lookup type or its entity type.</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the result and the refreshed identity (null if unchanged)</returns>
        /// <remarks>The generic arguments travel by name and are resolved on the server through its trusted wire types
        /// only, never by loading a type the caller has named.</remarks>
        Task<(TResult Result, ClaimsIdentity Identity)> FetchAsync<TArg, TResult>(Type useCaseType,
            TArg arg, Type[] genericArgs, ClaimsIdentity identity);

        /// <summary>Invokes a remote method with two parameters that returns a value.</summary>
        /// <typeparam name="TArg1">the type of the first parameter</typeparam>
        /// <typeparam name="TArg2">the type of the second parameter</typeparam>
        /// <typeparam name="TResult">the return type of the remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg1">the first argument</param>
        /// <param name="arg2">the second argument</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the result and the refreshed identity (null if unchanged)</returns>
        Task<(TResult Result, ClaimsIdentity Identity)> FetchAsync<TArg1, TArg2, TResult>(Type useCaseType,
            TArg1 arg1, TArg2 arg2, ClaimsIdentity identity);

        /// <summary>Invokes a remote method with three parameters that returns a value.</summary>
        /// <typeparam name="TArg1">the type of the first parameter</typeparam>
        /// <typeparam name="TArg2">the type of the second parameter</typeparam>
        /// <typeparam name="TArg3">the type of the third parameter</typeparam>
        /// <typeparam name="TResult">the return type of the remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg1">the first argument</param>
        /// <param name="arg2">the second argument</param>
        /// <param name="arg3">the third argument</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the result and the refreshed identity (null if unchanged)</returns>
        Task<(TResult Result, ClaimsIdentity Identity)> FetchAsync<TArg1, TArg2, TArg3, TResult>(
            Type useCaseType, TArg1 arg1, TArg2 arg2, TArg3 arg3, ClaimsIdentity identity);


        /// <summary>Invokes a remote method without parameters; a result, if any, is discarded.</summary>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the refreshed identity; null if unchanged</returns>
        Task<ClaimsIdentity> InvokeAsync(Type useCaseType, ClaimsIdentity identity);

        /// <summary>Invokes a remote method with one parameter; a result, if any, is discarded.</summary>
        /// <typeparam name="TArg">the type of the parameter</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg">the argument</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the refreshed identity; null if unchanged</returns>
        Task<ClaimsIdentity> InvokeAsync<TArg>(Type useCaseType, TArg arg, ClaimsIdentity identity);

        /// <summary>Invokes a remote method with two parameters; a result, if any, is discarded.</summary>
        /// <typeparam name="TArg1">the type of the first parameter</typeparam>
        /// <typeparam name="TArg2">the type of the second parameter</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg1">the first argument</param>
        /// <param name="arg2">the second argument</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the refreshed identity; null if unchanged</returns>
        Task<ClaimsIdentity> InvokeAsync<TArg1, TArg2>(Type useCaseType, TArg1 arg1, TArg2 arg2,
            ClaimsIdentity identity);

        /// <summary>Invokes a remote method with three parameters; a result, if any, is discarded.</summary>
        /// <typeparam name="TArg1">the type of the first parameter</typeparam>
        /// <typeparam name="TArg2">the type of the second parameter</typeparam>
        /// <typeparam name="TArg3">the type of the third parameter</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg1">the first argument</param>
        /// <param name="arg2">the second argument</param>
        /// <param name="arg3">the third argument</param>
        /// <param name="identity">the caller's identity, holding the token claim; null for an anonymous call</param>
        /// <returns>the refreshed identity; null if unchanged</returns>
        Task<ClaimsIdentity> InvokeAsync<TArg1, TArg2, TArg3>(Type useCaseType, TArg1 arg1, TArg2 arg2,
            TArg3 arg3, ClaimsIdentity identity);
    }
}

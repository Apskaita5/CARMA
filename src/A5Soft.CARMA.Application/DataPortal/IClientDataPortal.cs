using A5Soft.CARMA.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The client side of the data portal, as a use case sees it: invokes the remote method of a use case
    /// on the server and gets its result back. Implemented by <see cref="Client.ClientDataPortal"/>.
    /// </summary>
    /// <remarks>
    /// <para>A use case calls it only when <see cref="IsRemote"/> is true, and only from an app-level use
    /// case base class, never from a concrete use case: the base class runs the use case locally
    /// otherwise.</para>
    /// <para>Every method takes the use case type (its interface or implementation) and the arguments of
    /// its remote method, in signature order.</para>
    /// <para>Parameters: a domain object travels as its DTO and is rebuilt on the server; any
    /// <see cref="Domain.Files.FileContent"/> in the arguments, however deeply nested, is uploaded as a file. The caller
    /// keeps owning the files.</para>
    /// <para>Results: a domain object is rebuilt from its DTO on the client. A <see cref="Domain.Files.FileContent"/>
    /// result is a download, streamed as it arrives: the caller must dispose it.
    /// A file nested in a result is always null.</para>
    /// <para>Failures are thrown as the application exceptions the <see cref="IRemoteErrorMapper"/> maps
    /// them to, or as <see cref="RemoteCallException"/>; see <see cref="RemoteErrorKind"/>. A server that
    /// cannot be reached is a <see cref="DataPortalUnavailableException"/>. Nothing is retried.</para>
    /// </remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IClientDataPortal
    {
        /// <summary>
        /// Gets a value indicating whether use cases run remotely, i.e. whether the scope's endpoint has a remote
        /// <see cref="EndPoints.IDataPortalProxy"/>. When false, use cases run locally and the data portal must not be called.
        /// </summary>
        bool IsRemote { get; }


        /// <summary>Invokes a remote method without parameters that returns a value.</summary>
        /// <typeparam name="TResult">the return type of the remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="cancellationToken">cancellation token (if any)</param>
        /// <returns>the result of the remote method; a <see cref="FileContent"/> result must be disposed</returns>
        Task<TResult> FetchAsync<TResult>(Type useCaseType, CancellationToken cancellationToken = default);

        /// <summary>Invokes a remote method with one parameter that returns a value.</summary>
        /// <typeparam name="TArg">the type of the parameter</typeparam>
        /// <typeparam name="TResult">the return type of the remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg">the argument</param>
        /// <param name="cancellationToken">cancellation token (if any)</param>
        /// <returns>the result of the remote method; a <see cref="FileContent"/> result must be disposed</returns>
        Task<TResult> FetchAsync<TArg, TResult>(Type useCaseType, TArg arg, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invokes a generic remote method with one parameter that returns a value, closed over the generic arguments
        /// given.
        /// </summary>
        /// <typeparam name="TArg">the type of the parameter</typeparam>
        /// <typeparam name="TResult">the return type of the closed remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg">the argument</param>
        /// <param name="genericArgs">the generic arguments, each a trusted wire type</param>
        /// <param name="cancellationToken">cancellation token (if any)</param>
        /// <returns>the result of the remote method; a <see cref="FileContent"/> result must be disposed</returns>
        Task<TResult> FetchAsync<TArg, TResult>(Type useCaseType, TArg arg, Type[] genericArgs,
            CancellationToken cancellationToken = default);

        /// <summary>Invokes a remote method with two parameters that returns a value.</summary>
        /// <typeparam name="TArg1">the type of the first parameter</typeparam>
        /// <typeparam name="TArg2">the type of the second parameter</typeparam>
        /// <typeparam name="TResult">the return type of the remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg1">the first argument</param>
        /// <param name="arg2">the second argument</param>
        /// <param name="cancellationToken">cancellation token (if any)</param>
        /// <returns>the result of the remote method; a <see cref="FileContent"/> result must be disposed</returns>
        Task<TResult> FetchAsync<TArg1, TArg2, TResult>(Type useCaseType, TArg1 arg1, TArg2 arg2,
            CancellationToken cancellationToken = default);

        /// <summary>Invokes a remote method with three parameters that returns a value.</summary>
        /// <typeparam name="TArg1">the type of the first parameter</typeparam>
        /// <typeparam name="TArg2">the type of the second parameter</typeparam>
        /// <typeparam name="TArg3">the type of the third parameter</typeparam>
        /// <typeparam name="TResult">the return type of the remote method</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg1">the first argument</param>
        /// <param name="arg2">the second argument</param>
        /// <param name="arg3">the third argument</param>
        /// <param name="cancellationToken">cancellation token (if any)</param>
        /// <returns>the result of the remote method; a <see cref="FileContent"/> result must be disposed</returns>
        Task<TResult> FetchAsync<TArg1, TArg2, TArg3, TResult>(Type useCaseType, TArg1 arg1, TArg2 arg2,
            TArg3 arg3, CancellationToken cancellationToken = default);

        /// <summary>Invokes a remote method without parameters; a result is discarded.</summary>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        Task InvokeAsync(Type useCaseType);

        /// <summary>Invokes a remote method with one parameter; a result is discarded.</summary>
        /// <typeparam name="TArg">the type of the parameter</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg">the argument</param>
        Task InvokeAsync<TArg>(Type useCaseType, TArg arg);

        /// <summary>Invokes a remote method with two parameters; a result is discarded.</summary>
        /// <typeparam name="TArg1">the type of the first parameter</typeparam>
        /// <typeparam name="TArg2">the type of the second parameter</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg1">the first argument</param>
        /// <param name="arg2">the second argument</param>
        Task InvokeAsync<TArg1, TArg2>(Type useCaseType, TArg1 arg1, TArg2 arg2);

        /// <summary>Invokes a remote method with three parameters; a result is discarded.</summary>
        /// <typeparam name="TArg1">the type of the first parameter</typeparam>
        /// <typeparam name="TArg2">the type of the second parameter</typeparam>
        /// <typeparam name="TArg3">the type of the third parameter</typeparam>
        /// <param name="useCaseType">the use case interface or implementation type</param>
        /// <param name="arg1">the first argument</param>
        /// <param name="arg2">the second argument</param>
        /// <param name="arg3">the third argument</param>
        Task InvokeAsync<TArg1, TArg2, TArg3>(Type useCaseType, TArg1 arg1, TArg2 arg2, TArg3 arg3);
    }
}

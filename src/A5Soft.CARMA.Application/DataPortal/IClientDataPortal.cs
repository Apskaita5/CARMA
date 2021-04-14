using A5Soft.CARMA.Domain;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A base interface for client side data portal implementations.
    /// Enable remote method execution using single entry point at server.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IClientDataPortal 
    {

        /// <summary>
        /// Gets a value indicating whether the data portal implementation is configured
        /// for remote invocations. All the methods of a client data portal can only be invoked
        /// if this property is set to true.
        /// </summary>
        bool IsRemote { get; }


        /// <summary>
        /// Executes method of the use case specified remotely and return the result. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TResult">a type of result of the method executed (should be binary serializable)</typeparam>
        /// <param name="identity">identity of the user invoking the method</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a result of the method executed</returns>
        Task<TResult> FetchAsync<TResult>(Type useCaseType, ClaimsIdentity identity, 
            CancellationToken ct = default);

        /// <summary>
        /// Executes method of the use case specified remotely and return the result. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg">a type of the (only) argument for the use case InvokeAsync method
        /// (should be either an interface (all the way to primitive types), a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TResult">a type of result of the method executed (should be binary serializable)</typeparam>
        /// <param name="parameter">a value of the (only) argument for the use case InvokeAsync method</param>
        /// <param name="identity">identity of the user invoking the method</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a result of the method executed</returns>
        Task<TResult> FetchAsync<TArg, TResult>(Type useCaseType, TArg parameter, 
            ClaimsIdentity identity, CancellationToken ct = default);

        /// <summary>
        /// Executes method of the use case specified remotely and return the result. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg1">a type of the first argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg2">a type of the second argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TResult">a type of result of the method executed (should be binary serializable)</typeparam>
        /// <param name="firstParameter">a value of the first argument for the use case InvokeAsync method</param>
        /// <param name="secondParameter">a value of the second argument for the use case InvokeAsync method</param>
        /// <param name="identity">identity of the user invoking the method</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a result of the method executed</returns>
        Task<TResult> FetchAsync<TArg1, TArg2, TResult>(Type useCaseType, TArg1 firstParameter, 
            TArg2 secondParameter, ClaimsIdentity identity, CancellationToken ct = default);

        /// <summary>
        /// Executes method of the use case specified remotely and return the result. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg1">a type of the first argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg2">a type of the second argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg3">a type of the third argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TResult">a type of result of the method executed (should be binary serializable)</typeparam>
        /// <param name="firstParameter">a value of the first argument for the use case InvokeAsync method</param>
        /// <param name="secondParameter">a value of the second argument for the use case InvokeAsync method</param>
        /// <param name="thirdParameter">a value of the third argument for the use case InvokeAsync method</param>
        /// <param name="identity">identity of the user invoking the method</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a result of the method executed</returns>
        Task<TResult> FetchAsync<TArg1, TArg2, TArg3, TResult>(Type useCaseType,
            TArg1 firstParameter, TArg2 secondParameter, TArg3 thirdParameter, 
            ClaimsIdentity identity, CancellationToken ct = default);

        /// <summary>
        /// Executes method of the use case specified remotely and return the result. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <typeparam name="TResult">a type of result of the method executed (should be binary serializable)</typeparam>
        /// <returns>a result of the method executed</returns> 
        /// <remarks>Only for methods that do not require authentication and authorization,
        /// e.g. login, password reset etc.</remarks>
        Task<TResult> FetchUnauthenticatedAsync<TResult>(Type useCaseType, CancellationToken ct = default);

        /// <summary>
        /// Executes method of the use case specified remotely and return the result. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg">a type of the (only) argument for the use case InvokeAsync method
        /// (should be either an interface (all the way to primitive types), a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TResult">a type of result of the method executed (should be binary serializable)</typeparam>
        /// <param name="parameter">a value of the (only) argument for the use case InvokeAsync method</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a result of the method executed</returns>
        /// <remarks>Only for methods that do not require authentication and authorization,
        /// e.g. login, password reset etc.</remarks>
        Task<TResult> FetchUnauthenticatedAsync<TArg, TResult>(Type useCaseType, 
            TArg parameter, CancellationToken ct = default);

        /// <summary>
        /// Executes method of the use case specified remotely and return the result. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg1">a type of the first argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg2">a type of the second argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TResult">a type of result of the method executed (should be binary serializable)</typeparam>
        /// <param name="firstParameter">a value of the first argument for the use case InvokeAsync method</param>
        /// <param name="secondParameter">a value of the second argument for the use case InvokeAsync method</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a result of the method executed</returns> 
        /// <remarks>Only for methods that do not require authentication and authorization,
        /// e.g. login, password reset etc.</remarks>
        Task<TResult> FetchUnauthenticatedAsync<TArg1, TArg2, TResult>(Type useCaseType,
            TArg1 firstParameter, TArg2 secondParameter, CancellationToken ct = default) ;

        /// <summary>
        /// Executes method of the use case specified remotely and return the result. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg1">a type of the first argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg2">a type of the second argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg3">a type of the third argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TResult">a type of result of the method executed (should be binary serializable)</typeparam>
        /// <param name="firstParameter">a value of the first argument for the use case InvokeAsync method</param>
        /// <param name="secondParameter">a value of the second argument for the use case InvokeAsync method</param>
        /// <param name="thirdParameter">a value of the third argument for the use case InvokeAsync method</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a result of the method executed</returns>
        /// <remarks>Only for methods that do not require authentication and authorization,
        /// e.g. login, password reset etc.</remarks>
        Task<TResult> FetchUnauthenticatedAsync<TArg1, TArg2, TArg3, TResult>(Type useCaseType,
            TArg1 firstParameter, TArg2 secondParameter, TArg3 thirdParameter, CancellationToken ct = default);

        /// <summary>
        /// Executes (void) method of the use case specified remotely. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <param name="identity">identity of the user invoking the method</param>
        Task InvokeAsync(Type useCaseType, ClaimsIdentity identity);

        /// <summary>
        /// Executes (void) method of the use case specified remotely. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg">a type of the (only) argument for the use case InvokeAsync method
        /// (should be either an interface (all the way to primitive types), a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <param name="parameter">a value of the (only) argument for the use case InvokeAsync method</param>
        /// <param name="identity">identity of the user invoking the method</param>
        Task InvokeAsync<TArg>(Type useCaseType, TArg parameter, ClaimsIdentity identity);

        /// <summary>
        /// Executes (void) method of the use case specified remotely. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg1">a type of the first argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg2">a type of the second argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <param name="firstParameter">a value of the first argument for the use case InvokeAsync method</param>
        /// <param name="secondParameter">a value of the second argument for the use case InvokeAsync method</param>
        /// <param name="identity">identity of the user invoking the method</param>
        Task InvokeAsync<TArg1, TArg2>(Type useCaseType, TArg1 firstParameter, 
            TArg2 secondParameter, ClaimsIdentity identity);

        /// <summary>
        /// Executes (void) method of the use case specified remotely. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg1">a type of the first argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg2">a type of the second argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg3">a type of the third argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <param name="firstParameter">a value of the first argument for the use case InvokeAsync method</param>
        /// <param name="secondParameter">a value of the second argument for the use case InvokeAsync method</param>
        /// <param name="thirdParameter">a value of the third argument for the use case InvokeAsync method</param>
        /// <param name="identity">identity of the user invoking the method</param>
        Task InvokeAsync<TArg1, TArg2, TArg3>(Type useCaseType, TArg1 firstParameter, 
            TArg2 secondParameter, TArg3 thirdParameter, ClaimsIdentity identity);

        /// <summary>
        /// Executes (void) method of the use case specified remotely. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <remarks>Only for methods that do not require authentication and authorization,
        /// e.g. login, password reset etc.</remarks>
        Task InvokeUnauthenticatedAsync(Type useCaseType);

        /// <summary>
        /// Executes (void) method of the use case specified remotely. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg">a type of the (only) argument for the use case InvokeAsync method
        /// (should be either an interface (all the way to primitive types), a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <param name="parameter">a value of the (only) argument for the use case InvokeAsync method</param>
        /// <remarks>Only for methods that do not require authentication and authorization,
        /// e.g. login, password reset etc.</remarks>
        Task InvokeUnauthenticatedAsync<TArg>(Type useCaseType, TArg parameter);

        /// <summary>
        /// Executes (void) method of the use case specified remotely. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg1">a type of the first argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg2">a type of the second argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <param name="firstParameter">a value of the first argument for the use case InvokeAsync method</param>
        /// <param name="secondParameter">a value of the second argument for the use case InvokeAsync method</param>
        /// <remarks>Only for methods that do not require authentication and authorization,
        /// e.g. login, password reset etc.</remarks>
        Task InvokeUnauthenticatedAsync<TArg1, TArg2>(Type useCaseType, TArg1 firstParameter, TArg2 secondParameter);

        /// <summary>
        /// Executes (void) method of the use case specified remotely. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case or its interface</param>
        /// <typeparam name="TArg1">a type of the first argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg2">a type of the second argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <typeparam name="TArg3">a type of the third argument for the use case InvokeAsync method
        /// (should be either an interface all the way to primitive types, a primitive type
        /// or a json serializable POCO)</typeparam>
        /// <param name="firstParameter">a value of the first argument for the use case InvokeAsync method</param>
        /// <param name="secondParameter">a value of the second argument for the use case InvokeAsync method</param>
        /// <param name="thirdParameter">a value of the third argument for the use case InvokeAsync method</param>
        /// <remarks>Only for methods that do not require authentication and authorization,
        /// e.g. login, password reset etc.</remarks>
        Task InvokeUnauthenticatedAsync<TArg1, TArg2, TArg3>(Type useCaseType, TArg1 firstParameter, 
            TArg2 secondParameter, TArg3 thirdParameter);
    }
}
﻿using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A base interface for remote data portal implementations.
    /// </summary>
    [Service(ServiceLifetime.Singleton, typeof(LocalClientPortal))]
    public interface IRemoteClientPortal 
    {

        /// <summary>
        /// Gets a value indicating whether the data portal implementation is actually remote.
        /// </summary>
        bool IsRemote { get; }

        /// <summary>
        /// Calls a remote service (e.g. HTTP server) with the json serialized remote method invocation request
        /// and returns binary serialized remote invocation response.
        /// </summary>
        /// <param name="request">json serialized remote method invocation request</param>
        /// <returns>binary serialized remote invocation response</returns>
        /// <remarks>From the implementation point of view the underlying (serialized) types are not important.</remarks>
        Task<string> GetResponseAsync(string request);

    }
}

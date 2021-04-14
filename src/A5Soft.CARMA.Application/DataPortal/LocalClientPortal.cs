using A5Soft.CARMA.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Null implementation of IRemoteClientPortal for use by DI whn no remote server is required.
    /// </summary>
    [DefaultServiceImplementation(typeof(IRemoteClientPortal))]
    public class LocalClientPortal : IRemoteClientPortal
    {

        /// <inheritdoc cref="IRemoteClientPortal.IsRemote" />
        public bool IsRemote 
            => false;


        /// <inheritdoc cref="IRemoteClientPortal.GetResponseAsync" />
        public Task<string> GetResponseAsync(string request, CancellationToken ct = default)
        {
            throw new NotSupportedException("Local data portal cannot be invoked.");
        }

    }
}

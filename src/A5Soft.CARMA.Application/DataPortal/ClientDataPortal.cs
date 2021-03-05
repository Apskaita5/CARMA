using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain;
using Newtonsoft.Json;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A default implementation of IClientDataPortal that enables remote method invocation
    /// for a use case InvokeAsync method with different parameters sets.
    /// </summary>
    public sealed class ClientDataPortal : IClientDataPortal
    {
        private readonly IRemoteClientPortal _remoteClientPortal;


        public ClientDataPortal(IRemoteClientPortal remoteClientPortal)
        {
            _remoteClientPortal = remoteClientPortal;
        }


        /// <inheritdoc cref="IClientDataPortal.IsRemote" />
        public bool IsRemote => 
            _remoteClientPortal?.IsRemote ?? false;


        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task<TResult> InvokeAsync<TResult>(Type interfaceType, ClaimsIdentity identity) 
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(interfaceType, identity));

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task<TResult> InvokeAsync<TArg, TResult>(Type interfaceType, TArg parameter, 
            ClaimsIdentity identity) 
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest<TArg>(interfaceType, parameter, identity));

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task<TResult> InvokeAsync<TArg1, TArg2, TResult>(Type interfaceType,
            TArg1 firstParameter, TArg2 secondParameter, ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(interfaceType, firstParameter, 
                    secondParameter, identity));

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task<TResult> InvokeAsync<TArg1, TArg2, TArg3, TResult>(
            Type interfaceType, TArg1 firstParameter, TArg2 secondParameter, 
            TArg3 thirdParameter, ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(interfaceType, firstParameter, 
                    secondParameter, thirdParameter, identity));

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task<TResult> InvokeAsync<TResult>(Type interfaceType)
        {
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(interfaceType));

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task<TResult> InvokeAsync<TArg, TResult>(Type interfaceType, TArg parameter)
        {
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(interfaceType, parameter));

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task<TResult> InvokeAsync<TArg1, TArg2, TResult>(Type interfaceType, 
            TArg1 firstParameter, TArg2 secondParameter)
        {
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(interfaceType, firstParameter, secondParameter));

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task<TResult> InvokeAsync<TArg1, TArg2, TArg3, TResult>(Type interfaceType,
            TArg1 firstParameter, TArg2 secondParameter, TArg3 thirdParameter)
        {
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(interfaceType, firstParameter, 
                    secondParameter, thirdParameter));

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public Task InvokeAsync(Type interfaceType, ClaimsIdentity identity)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest(interfaceType, identity));
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task InvokeAsync<TArg>(Type interfaceType, TArg parameter, ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            _ = await InvokeAsync(DataPortalRequest.NewDataPortalRequest(interfaceType, parameter, identity));
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task InvokeAsync<TArg1, TArg2>(Type interfaceType, TArg1 firstParameter, 
            TArg2 secondParameter, ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            _ = await InvokeAsync(DataPortalRequest.NewDataPortalRequest(
                    interfaceType, firstParameter, secondParameter, identity));
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public async Task InvokeAsync<TArg1, TArg2, TArg3>(Type interfaceType, TArg1 firstParameter, 
            TArg2 secondParameter, TArg3 thirdParameter, ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            _ = await InvokeAsync(DataPortalRequest.NewDataPortalRequest<TArg1, TArg2, TArg3>(
                    interfaceType, firstParameter, secondParameter, thirdParameter, identity));
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public Task InvokeAsync(Type interfaceType)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest(interfaceType));
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public Task InvokeAsync<TArg>(Type interfaceType, TArg parameter)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest<TArg>(interfaceType, parameter));
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public Task InvokeAsync<TArg1, TArg2>(Type interfaceType, TArg1 firstParameter, TArg2 secondParameter)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest(interfaceType, 
                firstParameter, secondParameter));
        }

        /// <inheritdoc cref="o:IClientDataPortal.InvokeAsync" />
        public Task InvokeAsync<TArg1, TArg2, TArg3>(
            Type interfaceType, TArg1 firstParameter, TArg2 secondParameter, TArg3 thirdParameter)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest(interfaceType, 
                firstParameter, secondParameter, thirdParameter));
        }



        private async Task<DataPortalResponse> InvokeAsync(DataPortalRequest request)
        {
            if (!IsRemote) throw new InvalidOperationException(
                "Cannot invoke remote method on a data portal that is not remote.");

            var response = await _remoteClientPortal.GetResponseAsync(
                JsonConvert.SerializeObject(request));

            var result = BinaryDeserialize<DataPortalResponse>(response);

            if (result.ProcessingException != null) throw result.ProcessingException;

            return result;
        }

        private static T BinaryDeserialize<T>(string base64String)
        {
            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream(Convert.FromBase64String(base64String)))
            {
                return (T)formatter.Deserialize(ms);
            }
        }

        private void EnsureRemote()
        {
            if (!IsRemote) throw new InvalidOperationException(
                "Cannot invoke remote method on a data portal that is not remote.");
        }

    }
}

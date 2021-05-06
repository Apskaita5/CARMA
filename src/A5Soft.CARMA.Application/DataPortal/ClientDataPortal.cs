using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain;
using Newtonsoft.Json;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A default implementation of IClientDataPortal that enables remote method invocation
    /// for a use case InvokeAsync method with different parameters sets.
    /// </summary>
    [DefaultServiceImplementation(typeof(IClientDataPortal))]
    public sealed class ClientDataPortal : IClientDataPortal
    {
        private readonly IDataPortalProxy _dataPortalProxy;


        /// <summary>
        /// constructor for DI
        /// </summary>
        /// <param name="dataPortalProxy">remote client portal to use</param>
        public ClientDataPortal(IDataPortalProxy dataPortalProxy)
        {
            _dataPortalProxy = dataPortalProxy ?? throw new ArgumentNullException(nameof(dataPortalProxy));
        }


        /// <inheritdoc cref="IClientDataPortal.IsRemote" />
        public bool IsRemote => 
            _dataPortalProxy?.IsRemote ?? false;


        /// <inheritdoc cref="IClientDataPortal.FetchAsync{TResult}" />
        public async Task<TResult> FetchAsync<TResult>(Type useCaseType, 
            ClaimsIdentity identity, CancellationToken ct = default) 
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(useCaseType, identity), ct);

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="IClientDataPortal.FetchAsync{TArg, TResult}" />
        public async Task<TResult> FetchAsync<TArg, TResult>(Type useCaseType, TArg parameter, 
            ClaimsIdentity identity, CancellationToken ct = default) 
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest<TArg>(useCaseType, parameter, identity), ct);

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="IClientDataPortal.FetchAsync{TArg1, TArg2, TResult}" />
        public async Task<TResult> FetchAsync<TArg1, TArg2, TResult>(Type useCaseType,
            TArg1 firstParameter, TArg2 secondParameter, ClaimsIdentity identity, 
            CancellationToken ct = default)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(useCaseType, 
                    firstParameter, secondParameter, identity), ct);

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="IClientDataPortal.FetchAsync{TArg1,TArg2,TArg3,TResult}" />
        public async Task<TResult> FetchAsync<TArg1, TArg2, TArg3, TResult>(
            Type useCaseType, TArg1 firstParameter, TArg2 secondParameter, 
            TArg3 thirdParameter, ClaimsIdentity identity, CancellationToken ct = default)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(useCaseType, 
                    firstParameter, secondParameter, thirdParameter, identity), ct);

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="IClientDataPortal.FetchUnauthenticatedAsync{TResult}" />
        public async Task<TResult> FetchUnauthenticatedAsync<TResult>(Type useCaseType, 
            CancellationToken ct = default)
        {
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(useCaseType), ct);

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="IClientDataPortal.FetchUnauthenticatedAsync{TArg, TResult}" />
        public async Task<TResult> FetchUnauthenticatedAsync<TArg, TResult>(Type useCaseType, 
            TArg parameter, CancellationToken ct = default)
        {
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(useCaseType, parameter), ct);

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="IClientDataPortal.FetchUnauthenticatedAsync{TArg1, TArg2, TResult}" />
        public async Task<TResult> FetchUnauthenticatedAsync<TArg1, TArg2, TResult>(Type useCaseType, 
            TArg1 firstParameter, TArg2 secondParameter, CancellationToken ct = default)
        {
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(useCaseType, firstParameter, secondParameter), ct);

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="IClientDataPortal.FetchUnauthenticatedAsync{TArg1,TArg2,TArg3,TResult}" />
        public async Task<TResult> FetchUnauthenticatedAsync<TArg1, TArg2, TArg3, TResult>(Type useCaseType,
            TArg1 firstParameter, TArg2 secondParameter, TArg3 thirdParameter, CancellationToken ct = default)
        {
            var result = await InvokeAsync(
                DataPortalRequest.NewDataPortalRequest(useCaseType, firstParameter, 
                    secondParameter, thirdParameter), ct);

            return (TResult)result.Result;
        }

        /// <inheritdoc cref="IClientDataPortal.InvokeAsync" />
        public Task InvokeAsync(Type useCaseType, ClaimsIdentity identity)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest(useCaseType, 
                identity));
        }

        /// <inheritdoc cref="IClientDataPortal.InvokeAsync{TArg}" />
        public async Task InvokeAsync<TArg>(Type useCaseType, TArg parameter, ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            _ = await InvokeAsync(DataPortalRequest.NewDataPortalRequest(useCaseType, 
                parameter, identity));
        }

        /// <inheritdoc cref="IClientDataPortal.InvokeAsync{TArg1, TArg2}" />
        public async Task InvokeAsync<TArg1, TArg2>(Type useCaseType, TArg1 firstParameter, 
            TArg2 secondParameter, ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            _ = await InvokeAsync(DataPortalRequest.NewDataPortalRequest(
                    useCaseType, firstParameter, secondParameter, identity));
        }

        /// <inheritdoc cref="IClientDataPortal.InvokeAsync{TArg1, TArg2, TArg3}" />
        public async Task InvokeAsync<TArg1, TArg2, TArg3>(Type useCaseType, TArg1 firstParameter, 
            TArg2 secondParameter, TArg3 thirdParameter, ClaimsIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            
            _ = await InvokeAsync(DataPortalRequest.NewDataPortalRequest(useCaseType, 
                firstParameter, secondParameter, thirdParameter, identity));
        }

        /// <inheritdoc cref="IClientDataPortal.InvokeUnauthenticatedAsync" />
        public Task InvokeUnauthenticatedAsync(Type useCaseType)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest(useCaseType));
        }

        /// <inheritdoc cref="IClientDataPortal.InvokeUnauthenticatedAsync{TArg}" />
        public Task InvokeUnauthenticatedAsync<TArg>(Type useCaseType, TArg parameter)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest<TArg>(
                useCaseType, parameter));
        }

        /// <inheritdoc cref="IClientDataPortal.InvokeUnauthenticatedAsync{TArg1, TArg2}" />
        public Task InvokeUnauthenticatedAsync<TArg1, TArg2>(Type useCaseType, TArg1 firstParameter, TArg2 secondParameter)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest(useCaseType, 
                firstParameter, secondParameter));
        }

        /// <inheritdoc cref="IClientDataPortal.InvokeUnauthenticatedAsync{TArg1, TArg2, TArg3}" />
        public Task InvokeUnauthenticatedAsync<TArg1, TArg2, TArg3>(
            Type useCaseType, TArg1 firstParameter, TArg2 secondParameter, TArg3 thirdParameter)
        {
            return InvokeAsync(DataPortalRequest.NewDataPortalRequest(useCaseType, 
                firstParameter, secondParameter, thirdParameter));
        }



        private async Task<DataPortalResponse> InvokeAsync(DataPortalRequest request, 
            CancellationToken ct = default)
        {
            if (!IsRemote) throw new InvalidOperationException(
                "Cannot invoke remote method on a data portal that is not remote.");

            var response = await _dataPortalProxy.GetResponseAsync(
                JsonConvert.SerializeObject(request), ct);

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
        
    }
}

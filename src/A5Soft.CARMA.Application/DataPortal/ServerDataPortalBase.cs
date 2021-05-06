using A5Soft.CARMA.Domain;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// a base class for a server data portal
    /// </summary>
    public abstract class ServerDataPortalBase
    {
        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger Logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        protected ServerDataPortalBase(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <inheritdoc cref="IServerDataPortal.HandleRequest"/>
        public async Task<string> HandleRequest(string requestJson, Func<Type, object> getService,
            Action<ClaimsIdentity> setIdentity)
        {
            try
            {
                if (null == getService) throw new ArgumentNullException(nameof(getService));
                if (null == setIdentity) throw new ArgumentNullException(nameof(setIdentity));

                if (requestJson.IsNullOrWhiteSpace()) throw new ArgumentNullException(
                    "Null json request received on server.", nameof(requestJson));
                

                if (getService(typeof(IDataPortalProxy)) is IDataPortalProxy proxyPortal
                    && proxyPortal.IsRemote) return await proxyPortal.GetResponseAsync(requestJson);
                   
                var request = DeserializeRequest(requestJson);

                request.ValidateRequest();

                SetCulture(request.Culture, request.UICulture);

                var user = request.GetIdentity();
                if (user.IsAuthenticated)
                {
                    await ValidateIdentity(user);
                    setIdentity(user);
                }

                var service = getService(request.RemoteServiceInterface);

                if (null == service) throw new InvalidOperationException(
                    $"Remote service interface of type {request.RemoteServiceInterface.FullName} " +
                    $"is not available on server.");

                var method = request.GetRemoteMethod();

                var task = (Task)method.Invoke(service, request.GetRemoteMethodParamValues());

                if (null == task) throw new InvalidOperationException($"Remote service method " +
                    $"{method.Name} for interface of type {request.RemoteServiceInterface.FullName} " +
                    $"does not return Task.");

                await task.ConfigureAwait(false);

                var resultProp = task.GetType().GetProperty("Result");

                DataPortalResponse response;
                if (null == resultProp) response = new DataPortalResponse();
                else response = new DataPortalResponse() { Result = resultProp.GetValue(task) };

                return BinarySerialize(response);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, requestJson);
                var response = new DataPortalResponse()
                {
                    ProcessingException = GetExceptionForUser(ex)
                };
                return BinarySerialize(response);
            }
        }


        /// <summary>
        /// Override this method to set culture for the request processing.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="uiCulture"></param>
        protected abstract void SetCulture(CultureInfo culture, CultureInfo uiCulture);

        /// <summary>
        /// Override this method to validate user identity for the request.
        /// (only invoked if the user is authenticated)
        /// </summary>
        /// <param name="user">a request identity to validate</param>
        protected abstract Task ValidateIdentity(ClaimsIdentity user);
            
        /// <summary>
        /// Override this method to hide technical exceptions from user.
        /// </summary>
        /// <param name="actualException"></param>
        protected abstract Exception GetExceptionForUser(Exception actualException);

       
        private static string BinarySerialize<T>(T response)
        {
            byte[] bytes;
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, response);
                bytes = stream.ToArray();
            }

            return Convert.ToBase64String(bytes);
        }

        private static DataPortalRequest DeserializeRequest(string requestJson)
        {
            DataPortalRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<DataPortalRequest>(requestJson);
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                    $"Failed to deserialize data portal request with message - {e.Message}",
                    nameof(requestJson), e);
            }

            if (null == request) throw new ArgumentException(
                $"Failed to deserialize data portal request, null result for json.",
                nameof(requestJson));

            return request;
        }

    }
}

using A5Soft.CARMA.Domain;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A method wrapper for handling data portal requests on server.
    /// </summary>
    public static class ServerDataPortal
    {
        /// <summary>
        /// Handles data portal requests on server.
        /// </summary>
        /// <param name="requestJson">json serialized request parameters</param>
        /// <param name="authenticationType">a type (code) of the authentication schema for data portal on server</param>
        /// <param name="cultureSetter">a method to set CultureInfo.CurrentCulture for the request</param>
        /// <param name="uiCultureSetter">a method to set CultureInfo.CurrentUICulture for the request</param>
        /// <param name="identitySetter">a method to set user identity for the request</param>
        /// <param name="serviceResolver">a method to resolve a (DI) configured service by its interface</param>
        /// <param name="logger">a logger to use for logging exceptions</param>
        /// <returns>binary serialized method invocation response</returns>
        public static async Task<string> HandleRequest(string requestJson, string authenticationType, 
            Action<CultureInfo> cultureSetter, Action<CultureInfo> uiCultureSetter, 
            Action<ClaimsIdentity> identitySetter, Func<Type, object> serviceResolver,
            ILogger logger)
        {
            try
            {
                if (requestJson.IsNullOrWhiteSpace()) throw new ArgumentNullException(
                    "Null json request received on server.", nameof(requestJson));
                if (null == serviceResolver) throw new ArgumentNullException(
                    "Service resolver is not configured on server.", nameof(serviceResolver));

                if (serviceResolver(typeof(IRemoteClientPortal)) is IRemoteClientPortal proxyPortal
                    && proxyPortal.IsRemote) return await proxyPortal.GetResponseAsync(requestJson);

                if (authenticationType.IsNullOrWhiteSpace()) throw new ArgumentNullException(
                        "Authentication type is not configured on server.", nameof(requestJson));
                if (null == cultureSetter) throw new ArgumentNullException(
                        "Culture setter is not configured on server.", nameof(cultureSetter));
                if (null == uiCultureSetter) throw new ArgumentNullException(
                        "UI Culture setter is not configured on server.", nameof(uiCultureSetter));
                if (null == identitySetter) throw new ArgumentNullException(
                        "Identity setter is not configured on server.", nameof(identitySetter));

                var request = DeserializeRequest(requestJson);

                request.ValidateRequest();

                cultureSetter(request.Culture);
                uiCultureSetter(request.UICulture);

                identitySetter(request.GetIdentity(authenticationType));

                var useCase = serviceResolver(request.RemoteServiceInterface);

                if (null == useCase) throw new InvalidOperationException(
                    $"Remote service interface of type {request.RemoteServiceInterface.FullName} " +
                    $"is not available on server.");

                var method = request.GetRemoteMethod();

                var task = (Task)method.Invoke(useCase, request.GetRemoteMethodParamValues());
                
                await task.ConfigureAwait(false);

                var resultProp = task.GetType().GetProperty("Result");

                DataPortalResponse response;
                if (null == resultProp) response = new DataPortalResponse();
                else response = new DataPortalResponse() { Result = resultProp.GetValue(task) };

                return BinarySerialize(response);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Server data portal exception: {ex.Message} - for request:\r\n{requestJson}");
                var response = new DataPortalResponse()
                {
                    ProcessingException = ex
                };
                return BinarySerialize(response);
            }
        }

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

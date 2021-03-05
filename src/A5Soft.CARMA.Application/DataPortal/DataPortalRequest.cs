using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A serializable wrapper for the data portal request.
    /// </summary>
    [Serializable]
    internal class DataPortalRequest
    {

        #region Constructors

        public DataPortalRequest() { }

        private DataPortalRequest(Type remoteServiceInterface, ClaimsIdentity identity, 
            params DataPortalParameter[] parameters)
        {
            RemoteServiceInterface = remoteServiceInterface;

            if (!RemoteServiceInterface.IsInterface) throw new ArgumentException(
                $"Type {RemoteServiceInterface.FullName} is not an interface.", 
                nameof(RemoteServiceInterface));
            if (!RemoteServiceInterface.GetCustomAttributes<RemoteMethodAttribute>(false).Any()) 
                throw new ArgumentException(
                $"Interface {RemoteServiceInterface.FullName} is not a marked with a RemoteServiceAttribute.", 
                nameof(RemoteServiceInterface));

            Culture = CultureInfo.CurrentCulture;
            UICulture = CultureInfo.CurrentUICulture;

            if (null != identity && identity.IsAuthenticated)
                IdentityClaims = identity.Claims
                    .Select(c => new KeyValuePair<string, string>(c.Type, c.Value))
                    .ToList();
            else IdentityClaims = new List<KeyValuePair<string, string>>();

            if (null == parameters) UseCaseParams = new List<DataPortalParameter>();
            else UseCaseParams = new List<DataPortalParameter>(parameters);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a type of the remote service interface (to resolve configured version on server
        /// and invoke data portal method.)
        /// </summary>
        public Type RemoteServiceInterface { get; set; }

        /// <summary>
        /// Gets or sets user culture.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets user UI culture.
        /// </summary>
        public CultureInfo UICulture { get; set; }

        /// <summary>
        /// Gets or sets a list of claims for the user who invokes the remote method.
        /// </summary>
        public List<KeyValuePair<string, string>> IdentityClaims { get; set; }

        /// <summary>
        /// Gets or sets a list of (serialized) parameters for remote method invocation.
        /// </summary>
        public List<DataPortalParameter> UseCaseParams { get; set; }

        #endregion
        
        #region Helper Methods For Server Data Portal

        /// <summary>
        /// Gets a parameter signature for the remote method to invoke.
        /// </summary>
        /// <returns>a parameter signature for the remote method to invoke</returns>
        public Type[] GetRemoteMethodSignature()
        {
            if (null == UseCaseParams || UseCaseParams.Count < 1) return new Type[] { };
            return UseCaseParams.Select(p => p.ParameterType).ToArray();
        }

        /// <summary>
        /// Gets a description of parameter signature for the remote method to invoke.
        /// </summary>
        /// <returns>a description of parameter signature for the remote method to invoke</returns>
        public string GetRemoteMethodSignatureDescription()
        {
            if (null == UseCaseParams || UseCaseParams.Count < 1) return "without parameters";
            return string.Join(", ", UseCaseParams.Select(p => p.ParameterType.FullName));
        }

        /// <summary>
        /// Gets a number of parameters for the remote method.
        /// </summary>
        /// <returns>a number of parameters for the data portal method</returns>
        public int GetRemoteMethodParamCount()
        {
            return UseCaseParams?.Count ?? 0;
        }

        /// <summary>
        /// Gets parameters for the remote method to invoke (null for no params).
        /// </summary>
        /// <returns>parameters for the remote method to invoke (null for no params)</returns>
        public object[] GetRemoteMethodParamValues()
        {
            if (null == UseCaseParams || UseCaseParams.Count < 1) return null;
            return UseCaseParams.Select(p => p.GetValue()).ToArray();
        }

        /// <summary>
        /// Gets a ClaimsIdentity instance with the claims within the request.
        /// </summary>
        /// <param name="authenticationType">a type (code) of the data portal authentication on server</param>
        /// <returns>a ClaimsIdentity instance with the claims within the request</returns>
        public ClaimsIdentity GetIdentity(string authenticationType)
        {
            if (null == authenticationType || authenticationType.Trim().Length < 1)
                throw new ArgumentNullException(nameof(authenticationType));

            if (null != IdentityClaims && IdentityClaims.Count > 0)
            {
                return new ClaimsIdentity(IdentityClaims
                    .Select(p => new Claim(p.Key, p.Value)), authenticationType);
            }

            return new ClaimsIdentity();
        }

        /// <summary>
        /// Gets a method of the remote service interface that should be invoked by a server data portal.
        /// </summary>
        public MethodInfo GetRemoteMethod()
        {
            var method = RemoteServiceInterface.GetMethods()
                .FirstOrDefault(m => m.GetCustomAttributes<RemoteMethodAttribute>().Any());

            if (null == method) throw new InvalidOperationException(
                $"Remote method is not defined for remote service interface {RemoteServiceInterface.FullName}.");

            var requiredParams = method.GetParameters();
            var providedParams = GetRemoteMethodSignature();

            if (requiredParams.Length != providedParams.Length)
                throw new InvalidOperationException(
                    $"Parameter signature mismatch for remote service interface {RemoteServiceInterface.FullName}, " +
                    $"required (" +
                    $"{string.Join(", ", requiredParams.Select(p => p.ParameterType.FullName))}" +
                    $"), received (" +
                    $"{string.Join(", ", providedParams.Select(p => p.FullName))})");

            for (int i = 0; i < requiredParams.Length; i++)
            {
                if (requiredParams[i].ParameterType != providedParams[i]) 
                    throw new InvalidOperationException(
                    $"Parameter signature mismatch for remote service interface {RemoteServiceInterface.FullName}, " +
                    $"required (" +
                    $"{string.Join(", ", requiredParams.Select(p => p.ParameterType.FullName))}" +
                    $"), received (" +
                    $"{string.Join(", ", providedParams.Select(p => p.FullName))})");
            }

            return method;
        }

        /// <summary>
        /// Throws an ArgumentException if the request is invalid (shall not be
        /// processed by a server data portal).
        /// </summary>
        public void ValidateRequest()
        {
            if (null == RemoteServiceInterface) throw new ArgumentException(
                $"Remote service interface type is not specified in request.");
            if (!RemoteServiceInterface.IsInterface) throw new ArgumentException(
                $"Remote service interface type shall be an interface while type " +
                $"{RemoteServiceInterface.FullName} is not.");
            if (!RemoteServiceInterface.GetCustomAttributes(
                typeof(RemoteMethodAttribute), false).Any())
                throw new ArgumentException(
                    $"Remote service interface type {RemoteServiceInterface.FullName} " +
                    $"is not marked with a RemoteServiceAttribute.");
        }

        #endregion

        #region Static Factory Methods

        public static DataPortalRequest NewDataPortalRequest(Type interfaceType, ClaimsIdentity identity)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));

            return new DataPortalRequest(interfaceType, identity);
        }

        public static DataPortalRequest NewDataPortalRequest<TArg>(Type interfaceType, 
            TArg parameter, ClaimsIdentity identity)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));

            return new DataPortalRequest(interfaceType, identity,
                DataPortalParameter.NewParameter(parameter));
        }

        public static DataPortalRequest NewDataPortalRequest<TArg1, TArg2>(
            Type interfaceType, TArg1 firstParameter, TArg2 secondParameter, ClaimsIdentity identity)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));

            return new DataPortalRequest(interfaceType, identity,
                DataPortalParameter.NewParameter(firstParameter),
                DataPortalParameter.NewParameter(secondParameter));
        }

        public static DataPortalRequest NewDataPortalRequest<TArg1, TArg2, TArg3>(
            Type interfaceType, TArg1 firstParameter, TArg2 secondParameter, TArg3 thirdParameter, 
            ClaimsIdentity identity)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));

            return new DataPortalRequest(interfaceType, identity,
                DataPortalParameter.NewParameter(firstParameter),
                DataPortalParameter.NewParameter(secondParameter),
                DataPortalParameter.NewParameter(thirdParameter));
        }

        public static DataPortalRequest NewDataPortalRequest(Type interfaceType)
        {
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));
            return new DataPortalRequest(interfaceType, null);
        }

        public static DataPortalRequest NewDataPortalRequest<TArg>(Type interfaceType, TArg parameter)
        {
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));

            return new DataPortalRequest(interfaceType, null,
                DataPortalParameter.NewParameter(parameter));
        }

        public static DataPortalRequest NewDataPortalRequest<TArg1, TArg2>(
            Type interfaceType, TArg1 firstParameter, TArg2 secondParameter)
        {
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));

            return new DataPortalRequest(interfaceType, null,
                DataPortalParameter.NewParameter(firstParameter),
                DataPortalParameter.NewParameter(secondParameter));
        }

        public static DataPortalRequest NewDataPortalRequest<TArg1, TArg2, TArg3>(
            Type interfaceType, TArg1 firstParameter, TArg2 secondParameter, TArg3 thirdParameter)
        {
            if (null == interfaceType) throw new ArgumentNullException(nameof(interfaceType));

            return new DataPortalRequest(interfaceType, null,
                DataPortalParameter.NewParameter(firstParameter),
                DataPortalParameter.NewParameter(secondParameter),
                DataPortalParameter.NewParameter(thirdParameter));
        }

        #endregion

    }
}

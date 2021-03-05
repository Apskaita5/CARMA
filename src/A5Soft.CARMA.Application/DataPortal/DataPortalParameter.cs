using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Handles remote method parameter serialization.  
    /// </summary>
    /// <remarks>For primitive and POCO types serializes values into json strings.
    /// For interfaces uses JObject extensions to (de)serialize by interface without a concrete class.</remarks>
    [Serializable]
    internal class DataPortalParameter
    {
        
        /// <summary>
        /// Gets or sets a type of the remote method parameter.
        /// </summary>
        public Type ParameterType { get; set; }

        /// <summary>
        /// Gets or sets serialized parameter as a json string.
        /// </summary>
        public string SerializedValue { get; set; }


        /// <summary>
        /// Gets a deserialized value of the parameter.
        /// </summary>
        /// <returns>a deserialized value of the parameter</returns>
        /// <remarks>For primitive and POCO types serializes values into json strings.
        /// For interfaces uses JObject extensions to (de)serialize by interface without a concrete class.</remarks>
        public object GetValue()
        {
            if (null == ParameterType) throw new InvalidOperationException(
                "DataPortalParameter value type cannot be null.");

            if (null == SerializedValue) return null;

            if (ParameterType.IsInterface)
            {
                var args = JObject.Parse(SerializedValue);
                return args.toProxy(ParameterType);
            }

            return JsonConvert.DeserializeObject(SerializedValue, ParameterType);
        }

        /// <summary>
        /// Gets a deserialized value of the parameter.
        /// </summary>
        /// <typeparam name="T">a requested type of the parameter; must match ParameterType</typeparam>
        /// <returns>a deserialized value of the parameter</returns>
        /// <remarks>For primitive and POCO types serializes values into json strings.
        /// For interfaces uses JObject extensions to (de)serialize by interface without a concrete class.</remarks>
        public T GetValue<T>()
        {
            if (null == ParameterType) throw new InvalidOperationException(
                "DataPortalParameter value type cannot be null.");
            if (!typeof(T).IsAssignableFrom(ParameterType)) throw new InvalidCastException(
                $"Requested data portal parameter type {typeof(T).FullName} is not assignable from original parameter type {ParameterType.FullName}.");

            if (null == SerializedValue) return default;

            if (typeof(T).IsInterface)
            {
                var args = JObject.Parse(SerializedValue);
                return args.toProxy<T>();
            }

            return JsonConvert.DeserializeObject<T>(SerializedValue);
        }


        /// <summary>
        /// Creates an instance of DataPortalParameter for a remote method parameter value.
        /// </summary>
        /// <typeparam name="T">a type of the parameter</typeparam>
        /// <param name="forValue">a value of the parameter</param>
        /// <returns>an instance of DataPortalParameter for a remote method parameter value</returns>
        public static DataPortalParameter NewParameter<T>(T forValue)
        {
            return new DataPortalParameter()
            {
                ParameterType = typeof(T),
                SerializedValue = JsonConvert.SerializeObject(forValue)
            };
        }

    }
}

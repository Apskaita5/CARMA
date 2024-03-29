﻿using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static A5Soft.CARMA.Application.DataPortal.Extensions;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// https://www.lighthouselogic.com/deserialize-json-to-interface-without-concrete-class/
    /// </summary>
    public static class JObjectExtension
    {
        private static ProxyGenerator generator = new ProxyGenerator();
        private static JsonSerializer serializer = new JsonSerializer()
        {
            ContractResolver = new CustomResolver()
        };


        /// <summary>
        /// Converts JToken value to an interface proxy of the type specified.
        /// </summary>
        /// <typeparam name="TInterfaceType">a type of the interface to proxy</typeparam>
        /// <param name="targetObject">a JToken containing data for the interface proxy</param>
        /// <returns>an interface proxy of the type specified</returns>
        public static TInterfaceType ToProxy<TInterfaceType>(this JToken targetObject)
        {
            return (TInterfaceType)targetObject.ToProxy(typeof(TInterfaceType));
        }

        /// <summary>
        /// Converts JToken value to an interface proxy of the type specified.
        /// </summary>
        /// <param name="targetToken">a JToken containing data for the interface proxy</param>
        /// <param name="interfaceType">a type of the interface to proxy</param>
        /// <returns>an interface proxy of the type specified</returns>
        public static object ToProxy(this JToken targetToken, Type interfaceType)
        {
            if (interfaceType.GetTypeInfo().IsValueType || interfaceType.GetTypeInfo().IsPrimitive
                || interfaceType.Equals(typeof(string)) || interfaceType.GetTypeInfo().IsEnum)
            {
                return targetToken.ToObject(interfaceType);
            }

            if (interfaceType.Equals(typeof(decimal)))
            {
                //Due to a quirk in how newtonsoft handles large numbers, they need to be passed in as a string
                if (targetToken.Type == JTokenType.String)
                {
                    return Decimal.Parse(targetToken.Value<string>());
                }
                else
                {
                    return targetToken.ToObject(interfaceType);
                }
            }

            if (typeof(Stream).IsAssignableFrom(interfaceType))
            {
                var bytes = (byte[])targetToken.ToObject(typeof(byte[]));
                if (null == bytes || bytes.Length < 1) return null;
                return new MemoryStream(bytes);
            }

            if (typeof(JContainer).IsAssignableFrom(interfaceType))
            {
                return targetToken;
            }

            if (interfaceType.GetTypeInfo().IsClass)
            {
                try
                {
                    return targetToken.ToObject(interfaceType, serializer);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Cannot Proxy {interfaceType.FullName} - It is a concrete class, only interfaces can be proxied.", ex);
                }
            }

            return generator.CreateInterfaceProxyWithoutTarget(interfaceType, new[]
                { typeof(JTokenExposable), typeof(ShouldSerialize) },
                new JTokenInterceptor(targetToken, interfaceType));
        }


        private interface JTokenExposable
        {
            JToken getTargetJToken();
        }

        private interface ShouldSerialize
        {
            //Control serialization of Dynamic Proxy meta data
            bool ShouldSerialize__interceptors();
            bool ShouldSerialize__target();
        }

        private class JTokenInterceptor : IInterceptor
        {
            private JToken _target;
            internal Type _proxyType;

            public JTokenInterceptor(JToken target, Type proxyType)
            {
                _target = target;
                _proxyType = proxyType;
            }

            public void Intercept(IInvocation invocation)
            {
                var methodName = invocation.Method.Name;

                if (methodName == "getTargetJToken")
                {
                    invocation.ReturnValue = _target;
                    return;
                }

                if (methodName == "get_Count")
                {
                    invocation.ReturnValue = _target.Count();
                    return;
                }

                if (methodName == "get_Item")
                {
                    invocation.ReturnValue = _target[invocation.Arguments[0]].ToProxy(invocation.Method.ReturnType);
                    return;
                }

                if (methodName == "get_ChildrenTokens")
                {
                    invocation.ReturnValue = _target.Children().ToList();
                    return;
                }

                if (methodName == "GetEnumerator" && invocation.Method.ReturnType.IsConstructedGenericType)
                {
                    var returnType = invocation.Method.ReturnType.GetGenericArguments()[0];

                    Type enumeratorType = typeof(IEnumerator<>).MakeGenericType(returnType);
                    if (_target.Type == JTokenType.Array)
                    {
                        invocation.ReturnValue = JObjectExtension.generator.CreateInterfaceProxyWithoutTarget(enumeratorType, new EnumeratorInterceptor(returnType, this, ((JArray)_target).GetEnumerator()));
                    }
                    else
                    {
                        invocation.ReturnValue = JObjectExtension.generator.CreateInterfaceProxyWithoutTarget(enumeratorType, new EnumeratorInterceptor(returnType, this, ((JObject)_target).GetEnumerator()));
                    }

                    return;
                }

                if (methodName == "GetEnumerator")
                {
                    invocation.ReturnValue = JObjectExtension.generator.CreateInterfaceProxyWithoutTarget(typeof(IEnumerator), new EnumeratorInterceptor(typeof(IEnumerator), this, ((JArray)_target).GetEnumerator()));

                    return;
                }

                if (invocation.Method.IsSpecialName && methodName.StartsWith("get_"))
                {
                    var returnType = invocation.Method.ReturnType;
                    methodName = methodName.Substring(4);

                    if (_target == null || !_target.HasValues || _target[methodName] == null)
                    {
                        if (returnType.GetTypeInfo().IsPrimitive)
                        {

                            invocation.ReturnValue = Activator.CreateInstance(returnType);
                            return;
                        }

                        invocation.ReturnValue = null;
                        return;
                    }

                    if (_target[methodName] is JArray)
                    {
                        invocation.ReturnValue = ((JArray)_target[methodName]).ToProxy(returnType);
                        return;
                    }

                    invocation.ReturnValue = _target[methodName].ToProxy(returnType);
                    return;
                }

                if (invocation.Method.IsSpecialName && methodName.StartsWith("set_"))
                {
                    methodName = methodName.Substring(4);
                    var interfaceType = invocation.Arguments[0].GetType();

                    if (interfaceType.GetTypeInfo().IsPrimitive || interfaceType.Equals(typeof(string)) || interfaceType.Equals(typeof(decimal)) || interfaceType.GetTypeInfo().IsEnum)
                    {
                        _target[methodName] = new JValue(invocation.Arguments[0]);
                        return;
                    }

                    _target[methodName] = JObject.FromObject(invocation.Arguments[0]);
                    return;
                }

                if (methodName == "ShouldSerialize__interceptors" || methodName == "ShouldSerialize__target")
                {
                    // Prevent serialization of Castle Dynamic meta properties
                    invocation.ReturnValue = false;
                    return;
                }

                throw new NotImplementedException("Only get and set accessors are implemented in proxy");
            }
        }

        private class EnumeratorInterceptor : IInterceptor
        {
            private IEnumerator _targetEnumerator;
            private Type _enumeratorType;
            private JTokenInterceptor _parentInterceptor;

            public EnumeratorInterceptor(Type enumeratorType, JTokenInterceptor parentInterceptor, IEnumerator targetEnumerator)
            {
                _targetEnumerator = targetEnumerator;
                _enumeratorType = enumeratorType;
                _parentInterceptor = parentInterceptor;
            }

            public void Intercept(IInvocation invocation)
            {
                switch (invocation.Method.Name)
                {
                    case "MoveNext":
                        invocation.ReturnValue = _targetEnumerator.MoveNext();
                        return;
                    case "Dispose":
                        if (_enumeratorType.Name.StartsWith("KeyValuePair"))
                        {
                            ((IEnumerator<KeyValuePair<string, JToken>>)_targetEnumerator).Dispose();
                        }
                        else
                        {
                            ((IEnumerator<JToken>)_targetEnumerator).Dispose();
                        }
                        return;
                    case "get_Current":
                        var dictTypes = invocation.Method.ReturnType.GetGenericArguments();
                        if (dictTypes.Length <= 1)
                        {
                            var jTokenEnumerator = (IEnumerator<JToken>)_targetEnumerator;

                            invocation.ReturnValue = ((IEnumerator<JToken>)_targetEnumerator).Current.ToProxy(_parentInterceptor._proxyType.GenericTypeArguments[0]);
                            return;
                        }

                        if (dictTypes.Length == 2)
                        {
                            if (dictTypes[0].Name == "String")
                            {
                                var kvpEnumerator = (IEnumerator<KeyValuePair<string, JToken>>)_targetEnumerator;
                                dynamic returnKVP = Activator.CreateInstance(invocation.Method.ReturnType, kvpEnumerator.Current.Key, kvpEnumerator.Current.Value.ToProxy(dictTypes[1]));
                                invocation.ReturnValue = returnKVP;
                                return;
                            }
                            else
                            {
                                throw new NotImplementedException("Dictionaries must have a string key");
                            }
                        }

                        throw new NotImplementedException("Unknown Enumerator Type");

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}

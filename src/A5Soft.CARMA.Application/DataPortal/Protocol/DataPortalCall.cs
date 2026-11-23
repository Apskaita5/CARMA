using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// The description of a remote use case call, in the canonical, transport independent form:
    /// everything the server data portal needs to clear a call before any of its data is read.
    /// </summary>
    /// <remarks>
    /// <para>Created by the client data portal for a proxy to send, and by a server endpoint from
    /// what it has received. On the server every value is untrusted until the server data portal
    /// has checked it.</para>
    /// <para>A transport carries these values in whatever form suits it (the HTTP transport uses
    /// headers), and must deliver them before the call data, so that the server can refuse a call
    /// without receiving its files.</para>
    /// </remarks>
    public sealed class DataPortalCall
    {
        /// <summary>
        /// Creates a new call description.
        /// </summary>
        /// <param name="useCase">a name of the use case interface to invoke</param>
        /// <param name="parameterTypes">the wire type names of the remote method parameters,
        /// in signature order</param>
        /// <param name="token">the caller's token; null for an anonymous call</param>
        /// <param name="culture">a name of the caller's culture; null to keep the server's</param>
        /// <param name="uiCulture">a name of the caller's UI culture; null to keep the server's</param>
        /// <param name="fileCount">the number of files the call carries</param>/// <param name="genericArguments">the wire names of the generic arguments of a generic remote
        /// method; null or empty for a method that is not generic</param>
        /// <param name="genericArguments">the wire names of the generic arguments of a generic remote
        /// method; null or empty for a method that is not generic</param>
        /// <param name="pluginFingerprint">the caller's plugin fingerprint
        /// (<see cref="IoC.PluginCatalog.Fingerprint"/>); it must match the server's</param>
        public DataPortalCall(string useCase, IReadOnlyList<string> parameterTypes, string token,
            string culture, string uiCulture, int fileCount, IReadOnlyList<string> genericArguments = null,
            string pluginFingerprint = null)
        {
            if (fileCount < 0) throw new ArgumentOutOfRangeException(nameof(fileCount));

            UseCase = useCase;
            ParameterTypes = parameterTypes ?? Array.Empty<string>();
            Token = token;
            Culture = culture;
            UICulture = uiCulture;
            FileCount = fileCount;
            GenericArguments = genericArguments ?? Array.Empty<string>();
            PluginFingerprint = pluginFingerprint;
        }


        /// <summary>
        /// Gets a name of the use case interface to invoke (<c>Type.ToString()</c> of the interface).
        /// The server looks it up in its registry; it is never resolved as a type name.
        /// </summary>
        public string UseCase { get; }

        /// <summary>
        /// Gets the wire type names of the remote method parameters, in signature order. The server
        /// compares them with its own, so that a client and a server built from different versions
        /// are detected before any parameter is read.
        /// </summary>
        public IReadOnlyList<string> ParameterTypes { get; }

        /// <summary>
        /// Gets the caller's token, taken from the identity claim
        /// <see cref="IDataPortalClientOptions.TokenClaimType"/>; null for an anonymous call.
        /// </summary>
        public string Token { get; }

        /// <summary>Gets a name of the caller's culture; null or empty to keep the server's.</summary>
        public string Culture { get; }

        /// <summary>Gets a name of the caller's UI culture; null or empty to keep the server's.</summary>
        public string UICulture { get; }

        /// <summary>
        /// Gets the number of files the call carries after its payload.
        /// </summary>
        public int FileCount { get; }

        /// <summary>
        /// Gets the wire names of the generic arguments of a generic remote method, resolved on the server
        /// through <see cref="RemoteUseCaseRegistry.TrustedWireTypes"/> only; empty for a method that is not generic.
        /// </summary>
        public IReadOnlyList<string> GenericArguments { get; }

        /// <summary>
        /// Gets the caller's plugin fingerprint (<see cref="IoC.PluginCatalog.Fingerprint"/>); a call whose
        /// fingerprint differs from the server's is refused.
        /// </summary>
        public string PluginFingerprint { get; }
    }
}

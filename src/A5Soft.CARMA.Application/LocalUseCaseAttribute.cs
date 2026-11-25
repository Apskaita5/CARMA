using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Designates the interface of a local use case: one that always runs where it is invoked, e.g. the settings of
    /// a GUI app. It is registered as a service like any use case, but never goes through the data portal: no
    /// <see cref="RemoteMethodAttribute"/>, not in the remote use case registry.
    /// </summary>
    /// <remarks>
    /// <para>Its implementation derives from the same bases as a remote one (the bases run it locally, after the
    /// authorization) and implements <see cref="IUseCase"/> like every use case: the attribute on the interface,
    /// <see cref="UseCaseAttribute"/> or this one, says whether a use case is remote or local.</para>
    /// <para>Its interface declares exactly one authorization attribute, like a remote one; never mark it with
    /// <see cref="UseCaseAttribute"/> too. A local use case cannot request lookups: the lookup service authorizes
    /// remote use cases only.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class LocalUseCaseAttribute : ServiceAttribute
    {
        /// <summary>
        /// Designates a local use case interface.
        /// </summary>
        /// <param name="lifetime">the lifetime of the use case within the IoC container</param>
        public LocalUseCaseAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient) : base(lifetime) { }
    }
}

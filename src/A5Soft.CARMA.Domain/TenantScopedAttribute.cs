using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Marks a service interface (use cases included) as tenant scoped: it can be resolved only in a scope
    /// with a tenant, and a tenant plugin can override it for its tenants only. Unmarked services are app scoped.
    /// </summary>
    /// <remarks>Only for scoped and transient services: singletons are always app wide. Not inherited by
    /// derived interfaces: each use case interface states its own scope.</remarks>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class TenantScopedAttribute : Attribute { }
}

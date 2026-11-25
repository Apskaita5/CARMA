using System;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// Marks a use case interface that only an admin user may invoke.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class AllowAdminOnlyAttribute : Attribute { }
}

using System;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// Marks a use case interface that a user who has logged in, but not yet provided the second factor, may invoke:
    /// providing the second factor, logging out. Any authenticated user may invoke it as well.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class AllowSecondFactorPendingAttribute : Attribute { }
}

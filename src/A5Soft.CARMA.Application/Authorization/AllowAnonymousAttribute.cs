using System;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// Marks a use case interface that anyone may invoke, without logging in: e.g. login, registration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class AllowAnonymousAttribute : Attribute { }
}

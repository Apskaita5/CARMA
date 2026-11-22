using System;

namespace A5Soft.CARMA.Application.IoC.Tenants
{
    /// <summary>
    /// Two plugins overriding the same tenant scoped service: no tenant can be assigned both.
    /// </summary>
    public sealed class ExclusivePlugins
    {
        internal ExclusivePlugins(string first, string second, Type service)
        {
            First = first;
            Second = second;
            Service = service;
        }


        /// <summary>Gets the identifier of one plugin.</summary>
        public string First { get; }

        /// <summary>Gets the identifier of the other plugin.</summary>
        public string Second { get; }

        /// <summary>Gets the tenant scoped service both override.</summary>
        public Type Service { get; }


        /// <inheritdoc />
        public override string ToString() => $"{First} and {Second}, which both override {Service.FullName}";
    }
}

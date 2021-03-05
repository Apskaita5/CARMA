using System;
using System.Linq;

namespace A5Soft.CARMA.Application
{
    internal static class InternalExtensions
    {

        public static Type GetRemoteServiceInterfaceType(this Type implementationType)
        {
            if (null == implementationType) throw new ArgumentNullException(nameof(implementationType));

            var result = implementationType.GetInterfaces()
                .Where(t => t.GetCustomAttributes(typeof(RemoteServiceAttribute), false).Any())
                .ToList();

            if (result.Count < 1) throw new InvalidOperationException(
                $"Service implementation (class) of type {implementationType.FullName} " +
                $"does not implement an interface with a RemoteServiceAttribute.");

            if (result.Count > 1) throw new InvalidOperationException(
                $"Service implementation (class) of type {implementationType.FullName} " +
                $"implements more than one interface with a RemoteServiceAttribute: " +
                $"{string.Join(", ", result)}.");

            return result[0];
        }

    }
}

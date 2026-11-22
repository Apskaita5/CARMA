using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.IoC
{
    /// <summary>
    /// Marker interface for app assemblies (projects) to declare required dependencies:
    /// required services from external libraries, unmanaged assemblies that need to be loaded.
    /// Implement one class per assembly to provide dependency information (if needed).
    /// </summary>
    public interface IAssemblyDependencies
    {
        /// <summary>
        /// Gets a collection of external services required for the project assembly for IoC.
        /// </summary>
        public IReadOnlyCollection<ExternalServiceDescriptor> ExternalServices { get; }

        /// <summary>
        /// Gets a collection of unmanaged assemblies required for the project assembly
        /// (to be loaded at runtime).
        /// </summary>
        public IReadOnlyCollection<UnmanagedAssemblyDescriptor> UnmanagedAssemblies { get; }
    }
}

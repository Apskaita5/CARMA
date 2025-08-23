using A5Soft.CARMA.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Represents an interface that should be implemented by an infrastructure
    /// component to run health checks for itself.
    /// </summary>
    [Service(ServiceLifetime.Singleton, allowMultipleImplementations: true)]
    public interface IHealthCheck
    {
        /// <summary>
        /// Runs health checks for the infrastructure component and returns the check results.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>health check result for the specific infrastructure component</returns>
        Task<HealthCheckResult> CheckHealthAsync(CancellationToken ct = default);
    }
}

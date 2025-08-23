using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A description of a health check result for the application infrastructure component.
    /// </summary>
    [Serializable]
    public class HealthCheckResult
    {
        /// <summary>
        /// Creates a description of a health check result for <see cref="HealthCheckState.Ok"/>
        /// or <see cref="HealthCheckState.PoorPerformance"/>.
        /// </summary>
        /// <param name="componentName"><see cref="ComponentName"/></param>
        /// <param name="isPoorPerformance">whether the component is performing poorly, i.e.
        /// <see cref="HealthStatus"/> is <see cref="HealthCheckState.PoorPerformance"/>.</param>
        /// <param name="details">Details of the health check (if any)</param>
        public HealthCheckResult(string componentName, bool isPoorPerformance = false, string details = null)
        {
            if (componentName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(componentName));
            ComponentName = componentName;
            HealthStatus = isPoorPerformance ? HealthCheckState.PoorPerformance : HealthCheckState.Ok;
            Details = details;
            Exception = string.Empty;
        }

        /// <summary>
        /// Creates a description of a health check result for <see cref="HealthCheckState.Failed"/>.
        /// </summary>
        /// <param name="componentName"><see cref="ComponentName"/></param>
        /// <param name="ex">an exception thrown by the component</param>
        public HealthCheckResult(string componentName, Exception ex, string details = null)
        {
            if (componentName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(componentName));
            if (null == ex) throw new ArgumentNullException(nameof(ex));

            ComponentName = componentName;
            HealthStatus = HealthCheckState.Failed;
            Exception = ex.Message;
            if (details.IsNullOrWhiteSpace()) Details = ex.GetFullDescription();
            else Details = $"{details}\r\n\r\nException Details:\r\n\r\n{ex.GetFullDescription()}";
        }


        /// <summary>
        /// A name of the infrastructure component checked.
        /// </summary>
        public string ComponentName { get; }

        /// <summary>
        /// Whether the infrastructure component is healthy (functioning nominally)
        /// </summary>
        public HealthCheckState HealthStatus { get; }

        /// <summary>
        /// A description of the exception thrown by the component if it fails.
        /// </summary>
        public string Exception { get; }

        /// <summary>
        /// Details for the health check results.
        /// </summary>
        public string Details { get; }
    }
}

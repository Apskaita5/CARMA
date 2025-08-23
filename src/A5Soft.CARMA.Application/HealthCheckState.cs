namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Infrastructure component health state.
    /// </summary>
    public enum HealthCheckState
    {
        /// <summary>
        /// Component is functioning nominally.
        /// </summary>
        Ok,

        /// <summary>
        /// Component is functioning but the performance is poor.
        /// </summary>
        PoorPerformance,

        /// <summary>
        /// Component is not functioning.
        /// </summary>
        Failed
    }
}

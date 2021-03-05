namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Defines use case life time in a DI container.
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// created every time a use case is injected or requested
        /// </summary>
        Transient = 0,

        /// <summary>
        /// Created per scope.
        /// In a web application, every web request creates a new separated service scope.
        /// That means scoped use cases are generally created per web request.
        /// </summary>
        Scoped = 1,

        /// <summary>
        /// Created per DI container.
        /// That generally means that they are created only one time per application
        /// and then used for whole the application life time.
        /// </summary>
        Singleton = 2
    }
}

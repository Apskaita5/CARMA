using System;
using A5Soft.CARMA.Application.Authorization.Login;
using A5Soft.CARMA.Application.DataPortal.Internal;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Sessions
{
    /// <summary>
    /// The security settings of a server, read from the <c>SecurityConfiguration</c> section of the app configuration.
    /// A setting missing from the configuration keeps its default.
    /// </summary>
    /// <remarks>
    /// <code>
    /// "SecurityConfiguration": {
    ///   "SessionLifetime": "00:30:00",
    ///   "SlidingExpiration": true,
    ///   "MaxSessionLifetime": "12:00:00",
    ///   "KeyRotationInterval": "1.00:00:00",
    ///   "SecondFactor": "None",
    ///   "SecondFactorLifetime": "00:05:00",
    ///   "SecondFactorMaxAttempts": 5,
    ///   "MaxFailedLogins": 5,
    ///   "LockoutDuration": "00:15:00"
    /// }
    /// </code>
    /// </remarks>
    public class SecurityConfiguration : ConfigurationBase, ISecurityConfiguration
    {
        /// <inheritdoc />
        public override Type InterfaceType => typeof(ISecurityConfiguration);

        /// <inheritdoc />
        public override BuildConfiguration ForBuildConfiguration => BuildConfiguration.Server;

        /// <inheritdoc />
        public TimeSpan SessionLifetime { get; set; } = TimeSpan.FromMinutes(30);

        /// <inheritdoc />
        public bool SlidingExpiration { get; set; } = true;

        /// <inheritdoc />
        public TimeSpan MaxSessionLifetime { get; set; } = TimeSpan.FromHours(12);

        /// <inheritdoc />
        public TimeSpan KeyRotationInterval { get; set; } = TimeSpan.FromDays(1);

        /// <inheritdoc />
        public SecondFactorPolicy SecondFactor { get; set; } = SecondFactorPolicy.None;

        /// <inheritdoc />
        public TimeSpan SecondFactorLifetime { get; set; } = TimeSpan.FromMinutes(5);

        /// <inheritdoc />
        public int SecondFactorMaxAttempts { get; set; } = 5;

        /// <inheritdoc />
        public int MaxFailedLogins { get; set; } = 5;

        /// <inheritdoc />
        public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();

            var v = new OptionsValidation(nameof(SecurityConfiguration));
            v.Require(SessionLifetime >= TimeSpan.FromMinutes(15) && SessionLifetime <= TimeSpan.FromDays(30),
                $"{nameof(SessionLifetime)} must be from 15 minutes to 30 days");
            v.Require(MaxSessionLifetime >= SessionLifetime && MaxSessionLifetime <= TimeSpan.FromDays(365),
                $"{nameof(MaxSessionLifetime)} must be at least {nameof(SessionLifetime)}, and at most 365 days");
            v.Require(KeyRotationInterval >= TimeSpan.FromHours(4) && KeyRotationInterval <= TimeSpan.FromDays(365),
                $"{nameof(KeyRotationInterval)} must be from 4 hours to 365 days");
            v.Require(Enum.IsDefined(typeof(SecondFactorPolicy), SecondFactor), $"{nameof(SecondFactor)} is not valid");
            v.Require(SecondFactorLifetime >= TimeSpan.FromMinutes(1) && SecondFactorLifetime <= TimeSpan.FromHours(1),
                $"{nameof(SecondFactorLifetime)} must be from 1 minute to 1 hour");
            v.Range(SecondFactorMaxAttempts, 1, 100, nameof(SecondFactorMaxAttempts));
            v.Range(MaxFailedLogins, 1, 1000, nameof(MaxFailedLogins));
            v.Require(LockoutDuration >= TimeSpan.Zero && LockoutDuration <= TimeSpan.FromDays(1),
                $"{nameof(LockoutDuration)} must be from 0 to 1 day");
            v.ThrowIfInvalid();
        }
    }
}

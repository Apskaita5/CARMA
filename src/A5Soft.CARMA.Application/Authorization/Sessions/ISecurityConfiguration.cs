using A5Soft.CARMA.Application.Authorization.Login;
using System;

namespace A5Soft.CARMA.Application.Authorization.Sessions
{
    /// <summary>
    /// The security settings of a server: sessions, second factor, lockout (section <c>SecurityConfiguration</c>).
    /// </summary>
    public interface ISecurityConfiguration
    {
        /// <summary>Gets how long a session lasts; with sliding expiration, since its last call.</summary>
        TimeSpan SessionLifetime { get; }

        /// <summary>Gets a value indicating whether every call extends the session (a new ticket on every call).</summary>
        bool SlidingExpiration { get; }

        /// <summary>Gets the longest a session can last, however it slides.</summary>
        TimeSpan MaxSessionLifetime { get; }

        /// <summary>Gets how often the default identity store makes a new signing key.</summary>
        TimeSpan KeyRotationInterval { get; }

        /// <summary>Gets who must provide a second factor at login.</summary>
        SecondFactorPolicy SecondFactor { get; }

        /// <summary>Gets how long a user has to provide the second factor after logging in.</summary>
        TimeSpan SecondFactorLifetime { get; }

        /// <summary>Gets how many wrong second factor codes end the pending session.</summary>
        int SecondFactorMaxAttempts { get; }

        /// <summary>Gets how many failed logins in a row lock a user name out.</summary>
        int MaxFailedLogins { get; }

        /// <summary>Gets how long a user name stays locked out.</summary>
        TimeSpan LockoutDuration { get; }
    }
}

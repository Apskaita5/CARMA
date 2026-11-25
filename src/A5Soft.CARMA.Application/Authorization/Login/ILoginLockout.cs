using System;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Login
{
    /// <summary>
    /// Counts failed attempts per key (a user name, a pending session) and locks the key out after too many, as
    /// <see cref="ISecurityConfiguration"/> says. The framework's <see cref="Default.DefaultLoginLockout"/> counts in
    /// memory (one server); an app with several servers implements a shared one.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface ILoginLockout
    {
        /// <summary>Gets how long the key stays locked out; null if it is not locked out.</summary>
        TimeSpan? GetLockout(string key);

        /// <summary>Records a failed attempt.</summary>
        /// <returns>the number of failed attempts in a row, this one included</returns>
        int RecordFailure(string key);

        /// <summary>Records a successful attempt: the count starts again.</summary>
        void RecordSuccess(string key);
    }
}

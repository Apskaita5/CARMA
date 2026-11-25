using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Login
{
    /// <summary>
    /// The second factor of a login: sends a code (email, SMS) or verifies one (an authenticator app). Implemented by
    /// the app, for server builds, if <see cref="ISecurityConfiguration.SecondFactor"/> is not
    /// <see cref="SecondFactorPolicy.None"/>.
    /// </summary>
    [Service(ServiceLifetime.Scoped)]
    public interface ISecondFactorProvider
    {
        /// <summary>
        /// Gets a value indicating whether the user must provide a second factor, beyond what the policy requires
        /// (<see cref="SecondFactorPolicy.PerUser"/>, <see cref="SecondFactorPolicy.Admins"/>).
        /// </summary>
        Task<bool> IsRequiredAsync(CarmaIdentity identity, CancellationToken cancellationToken);

        /// <summary>Sends the user a code, if the method sends one; does nothing for an authenticator app.</summary>
        Task SendCodeAsync(CarmaIdentity identity, CancellationToken cancellationToken);

        /// <summary>Verifies the code the user has provided.</summary>
        Task<bool> VerifyCodeAsync(CarmaIdentity identity, string code, CancellationToken cancellationToken);
    }
}

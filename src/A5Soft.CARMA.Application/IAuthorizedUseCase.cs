using System.Security.Claims;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Base interface for all use cases that implement authorization.
    /// </summary>
    public interface IAuthorizedUseCase : IUseCase
    {
        /// <summary>
        /// Gets a current identity of the user.
        /// </summary>
        Task<ClaimsIdentity> GetIdentityAsync();
              
        /// <summary>
        /// Gets a value indicating whether the current user is authorized to invoke the use case.
        /// </summary>
        /// <param name="throwOnNotAuthorized">whether to throw a (security) exception
        /// if the user is not authorized</param>
        /// <returns>a value indicating whether the user is authorized to invoke the use case</returns>
        Task<bool> CanInvokeAsync(bool throwOnNotAuthorized = false);
    }
}

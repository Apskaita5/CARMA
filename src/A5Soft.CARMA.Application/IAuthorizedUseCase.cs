using System.Security.Claims;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Base interface for all use cases that implement authorization.
    /// </summary>
    public interface IAuthorizedUseCase : IUseCase 
    {

        /// <summary>
        /// Gets an identity of the user of the use case.
        /// </summary>
        ClaimsIdentity User { get; }

        
        /// <summary>
        /// Gets a value indicating if the user is authorized to invoke the use case.
        /// </summary>
        /// <param name="throwOnNotAuthorized">whether to throw a (security) exception
        /// if the user is not authorized</param>
        /// <returns>a value indicating if the user is authorized to invoke the use case</returns>
        bool CanInvoke(bool throwOnNotAuthorized = false);

    }
}

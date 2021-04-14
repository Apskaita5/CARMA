using System;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Used to designate a use case interface that should be added to IoC container.
    /// Sets localizable name and description of the use case interface.
    /// Can use it as is by setting ResourceType value in the attribute decorator
    /// or inherit this class and set ResourceType in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class UseCaseAttribute : RemoteServiceAttribute
    {
        #region All Constructors

        /// <summary>
        /// Default constructor for UseCaseAttribute.
        /// </summary>
        /// <param name="name">the Name attribute property, which may be a resource key string</param>
        /// <param name="description">the Description attribute property, which may be a resource key string</param>
        /// <param name="resourceType">the <see cref="System.Type"/> that contains the resources for
        /// <see cref="Name"/> and <see cref="Description"/></param>
        /// <param name="lifetime">a lifetime of the use case within an IoC container</param>
        /// <param name="requiredLookups">types of lookups that the user needs (to access)
        /// in order to use this use case</param>
        public UseCaseAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient,
            params Type[] requiredLookups) : base(lifetime)
        {
            LookupTypes = requiredLookups;
        }

        #endregion

        #region Properties
                     
        /// <summary>
        /// Gets types of lookup values that the use case can request.
        /// </summary>
        public Type[] LookupTypes { get; }

        #endregion
        
    }
}

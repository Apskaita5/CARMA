using A5Soft.CARMA.Domain.Rules;
using System;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base class for all use cases.
    /// </summary>
    /// <remarks>
    /// All use cases need logging.
    /// Pretty much all use cases deal with business entities and require
    /// validation and metadata access for that purpose.</remarks>
    public abstract class UseCaseBase : IUseCase
    {
        /// <summary>
        /// validation provider
        /// </summary>
        protected readonly IValidationEngineProvider ValidationProvider;
        
        /// <summary>
        /// metadata provider
        /// </summary>
        protected readonly IMetadataProvider MetadataProvider;
        
        /// <summary>
        /// metadata provider
        /// </summary>
        protected readonly ILogger Logger;


        protected UseCaseBase(IValidationEngineProvider validationProvider, 
            IMetadataProvider metadataProvider, ILogger logger)
        {
            ValidationProvider = validationProvider ??
                throw new ArgumentNullException(nameof(validationProvider));
            MetadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}

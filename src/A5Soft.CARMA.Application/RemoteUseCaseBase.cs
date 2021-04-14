using A5Soft.CARMA.Application.DataPortal;
using System;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base class for use cases that support remote execution.
    /// </summary>
    public class RemoteUseCaseBase : UseCaseBase
    {
        /// <summary>
        /// data portal
        /// </summary>
        protected readonly IClientDataPortal DataPortal;


        /// <inheritdoc />
        public RemoteUseCaseBase(IClientDataPortal dataPortal, IValidationEngineProvider validationProvider, 
            IMetadataProvider metadataProvider, ILogger logger) : base(validationProvider, metadataProvider, logger)
        {
            DataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
        }

    }
}

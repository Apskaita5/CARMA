using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Domain.Test.Helpers.Utilities
{
    /// <summary>
    /// Base class for domain object tests providing common setup and utilities.
    /// </summary>
    public abstract class DomainObjectTestBase : IDisposable
    {
        protected Mock<IValidationEngineProvider> MockValidationProvider { get; }
        protected Mock<IMetadataProvider> MockLocalizationProvider { get; }

        protected DomainObjectTestBase()
        {
            MockValidationProvider = new Mock<IValidationEngineProvider>();
            MockLocalizationProvider = new Mock<IMetadataProvider>();
            
            // Default localization returns error codes as-is
            //MockLocalizationProvider
            //    .Setup(x => x.(It.IsAny<string>(), It.IsAny<object[]>()))
            //    .Returns<string, object[]>((key, args) => key);
            //);
        }

        public virtual void Dispose()
        {
            // Cleanup if needed
        }

        /// <summary>
        /// Helper to capture PropertyChanged events
        /// </summary>
        protected PropertyChangeTracker TrackPropertyChanges(INotifyPropertyChanged target)
        {
            return new PropertyChangeTracker(target);
        }
    }
}

using System.ComponentModel.DataAnnotations;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Interface implemented by bindable objects (entities and their lists)
    /// to ensure consistent event raising behaviour for all entities in a graph.
    /// </summary>
    public interface IBindable
    {
        /// <summary>
        /// Enables or disables INotifyPropertyChanged interface for the class instance.
        /// </summary>
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        bool NotifyPropertyChangedEnabled { get; set; }

        /// <summary>
        /// Enables or disables INotifyPropertyChanging interface for the class instance.
        /// </summary>
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        bool NotifyPropertyChangingEnabled { get; set; }

        /// <summary>
        /// Gets of sets a binding mode to adapt to a particular GUI framework.
        /// E.g. for WinForms binding schema there should be only one PropertyChanged event
        /// raised per range of changed properties, while for WPF binding schema an event per
        /// each property in range should be raised.
        /// </summary>
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        BindingMode BindingMode { get; set; }

    }
}

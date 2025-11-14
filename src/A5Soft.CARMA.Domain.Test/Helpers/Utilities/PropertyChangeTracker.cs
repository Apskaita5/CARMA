using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace A5Soft.CARMA.Domain.Test.Helpers.Utilities
{
    /// <summary>
    /// Tracks property change events for testing
    /// </summary>
    public class PropertyChangeTracker : IDisposable
    {
        private readonly INotifyPropertyChanged _target;
        private readonly List<string> _changedProperties = new();

        public PropertyChangeTracker(INotifyPropertyChanged target)
        {
            _target = target;
            _target.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _changedProperties.Add(e.PropertyName ?? string.Empty);
        }

        public IReadOnlyList<string> ChangedProperties => _changedProperties;

        public bool WasRaised(string propertyName) => _changedProperties.Contains(propertyName);

        public int CountFor(string propertyName) => _changedProperties.Count(p => p == propertyName);

        public void Reset() => _changedProperties.Clear();

        public void Dispose()
        {
            _target.PropertyChanged -= OnPropertyChanged;
        }
    }
}

using A5Soft.CARMA.Domain.Metadata.DataAnnotations;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// This class implements INotifyPropertyChanged  and INotifyPropertyChanging in a 
    /// serialization-safe manner.
    /// </summary>
    [Serializable]
    public abstract class BindableBase : INotifyPropertyChanged, INotifyPropertyChanging, IDomainObject, IBindable
    {
        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        protected BindableBase() { }


        private bool _notifyPropertyChangedEnabled = false;
        private bool _notifyPropertyChangingEnabled = false;
        private BindingMode _bindingMode = BindingMode.WinForms;
        [NonSerialized]
        private bool _suspendBinding = false;


        /// <inheritdoc cref="IBindable.NotifyPropertyChangedEnabled" />
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public bool NotifyPropertyChangedEnabled
        {
            get
            {
                return _notifyPropertyChangedEnabled;
            }
            set
            {
                if (_notifyPropertyChangedEnabled != value)
                {
                    _notifyPropertyChangedEnabled = value;
                    OnNotifyPropertyChangedEnabledChanged();
                }
            }
        }

        /// <inheritdoc cref="IBindable.NotifyPropertyChangingEnabled" />
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public bool NotifyPropertyChangingEnabled
        {
            get
            {
                return _notifyPropertyChangingEnabled;
            }
            set
            {
                if (_notifyPropertyChangingEnabled != value)
                {
                    _notifyPropertyChangingEnabled = value;
                    OnNotifyPropertyChangingEnabledChanged();
                }
            }
        }

        /// <inheritdoc cref="IBindable.BindingMode" />
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public BindingMode BindingMode
        {
            get
            {
                return _bindingMode;
            }
            set
            {
                if (_bindingMode != value)
                {
                    _bindingMode = value;
                    OnBindingModeChanged();
                }
            }
        }


        /// <summary>
        /// Override this method to update child objects bindings.
        /// </summary>
        protected virtual void OnNotifyPropertyChangedEnabledChanged() { }

        /// <summary>
        /// Override this method to update child objects bindings.
        /// </summary>
        protected virtual void OnNotifyPropertyChangingEnabledChanged() { }

        /// <summary>
        /// Override this method to update child objects bindings.
        /// </summary>
        protected virtual void OnBindingModeChanged() { }

        /// <summary>
        /// apply with using pattern to temporally disable binding events
        /// </summary>
        public IDisposable SuspendBindings()
            => new SuspendBindingsInt(this);

        #region INotifyPropertyChanged

        [NonSerialized()]
        private PropertyChangedEventHandler _nonSerializableChangedHandlers;
        private PropertyChangedEventHandler _serializableChangedHandlers;

        /// <summary>
        /// Implements a serialization-safe PropertyChanged event.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
          "CA1062:ValidateArgumentsOfPublicMethods")]
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (ShouldHandlerSerialize(value))
                    _serializableChangedHandlers = (PropertyChangedEventHandler)
                      System.Delegate.Combine(_serializableChangedHandlers, value);
                else
                    _nonSerializableChangedHandlers = (PropertyChangedEventHandler)
                      System.Delegate.Combine(_nonSerializableChangedHandlers, value);
            }
            remove
            {
                if (ShouldHandlerSerialize(value))
                    _serializableChangedHandlers = (PropertyChangedEventHandler)
                      System.Delegate.Remove(_serializableChangedHandlers, value);
                else
                    _nonSerializableChangedHandlers = (PropertyChangedEventHandler)
                      System.Delegate.Remove(_nonSerializableChangedHandlers, value);
            }
        }


        /// <summary>
        /// Override this method to change the default logic for determining 
        /// if the event handler should be serialized
        /// </summary>
        /// <param name="value">the event handler to review</param>
        protected virtual bool ShouldHandlerSerialize(PropertyChangedEventHandler value)
        {
            return value.Method.IsPublic &&
                   value.Method.DeclaringType != null &&
                   (value.Method.DeclaringType.IsSerializable || value.Method.IsStatic);
        }

        /// <summary>
        /// Call this method to raise the PropertyChanged event for a specific property.
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed.</param>
        /// <remarks>
        /// This method may be called by properties in the business  class to indicate the change in a specific property.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (NotifyPropertyChangedEnabled && !_suspendBinding)
            {
                _nonSerializableChangedHandlers?.Invoke(this,
                    new PropertyChangedEventArgs(propertyName));
                _serializableChangedHandlers?.Invoke(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Call this method to raise the PropertyChanged event for the properties specified.
        /// </summary>
        /// <param name="propertyNames">Names of the properties that has changed.</param>
        /// <remarks>
        /// This method may be called by methods in the business class to indicate the change in multiple properties.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPropertiesChanged(params string[] propertyNames)
        {
            if (NotifyPropertyChangedEnabled && !_suspendBinding)
            {
                if (_bindingMode == BindingMode.WinForms)
                {
                    _nonSerializableChangedHandlers?.Invoke(this,
                        new PropertyChangedEventArgs(propertyNames[0]));
                    _serializableChangedHandlers?.Invoke(this,
                        new PropertyChangedEventArgs(propertyNames[0]));
                }
                else
                {
                    foreach (var propertyName in propertyNames)
                    {
                        _nonSerializableChangedHandlers?.Invoke(this,
                            new PropertyChangedEventArgs(propertyName));
                        _serializableChangedHandlers?.Invoke(this,
                            new PropertyChangedEventArgs(propertyName));
                    }
                }
            }
        }

        /// <summary>
        /// Call this method to raise the PropertyChanged event for all object properties.
        /// </summary>
        /// <remarks>
        /// This method is automatically called by MarkDirty. It actually raises PropertyChanged for an empty string,
        /// which tells data binding to refresh all properties.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnUnknownPropertyChanged()
        {
            OnPropertyChanged(string.Empty);
        }

        #endregion

        #region INotifyPropertyChanging

        [NonSerialized()]
        private PropertyChangingEventHandler _nonSerializableChangingHandlers;
        private PropertyChangingEventHandler _serializableChangingHandlers;

        /// <summary>
        /// Implements a serialization-safe PropertyChanging event.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
          "CA1062:ValidateArgumentsOfPublicMethods")]
        public event PropertyChangingEventHandler PropertyChanging
        {
            add
            {
                if (ShouldHandlerSerialize(value))
                    _serializableChangingHandlers = (PropertyChangingEventHandler)
                      System.Delegate.Combine(_serializableChangingHandlers, value);
                else
                    _nonSerializableChangingHandlers = (PropertyChangingEventHandler)
                      System.Delegate.Combine(_nonSerializableChangingHandlers, value);
            }
            remove
            {
                if (ShouldHandlerSerialize(value))
                    _serializableChangingHandlers = (PropertyChangingEventHandler)
                      System.Delegate.Remove(_serializableChangingHandlers, value);
                else
                    _nonSerializableChangingHandlers = (PropertyChangingEventHandler)
                      System.Delegate.Remove(_nonSerializableChangingHandlers, value);
            }
        }


        /// <summary>
        /// Call this method to raise the PropertyChanging event for all object properties.
        /// </summary>
        /// <remarks>
        /// This method is automatically called by MarkDirty. It actually raises PropertyChanging for an empty string,
        /// which tells data binding to refresh all properties.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnUnknownPropertyChanging()
        {
            OnPropertyChanging(string.Empty);
        }

        /// <summary>
        /// Call this method to raise the PropertyChanging event for a specific property.
        /// </summary>
        /// <param name="propertyName">Name of the property that has Changing.</param>
        /// <remarks>
        /// This method may be called by properties in the business class to indicate the change in a specific property.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (NotifyPropertyChangingEnabled && !_suspendBinding)
            {
                _nonSerializableChangingHandlers?.Invoke(this,
                    new PropertyChangingEventArgs(propertyName));
                _serializableChangingHandlers?.Invoke(this,
                    new PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Call this method to raise the PropertyChanging event for the properties specified.
        /// </summary>
        /// <param name="propertyNames">Names of the properties that has Changing.</param>
        /// <remarks>
        /// This method may be called by methods in the business class to indicate the change in multiple properties.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPropertiesChanging(params string[] propertyNames)
        {
            if (NotifyPropertyChangingEnabled && !_suspendBinding)
            {
                foreach (var propertyName in propertyNames)
                {
                    _nonSerializableChangingHandlers?.Invoke(this,
                        new PropertyChangingEventArgs(propertyName));
                    _serializableChangingHandlers?.Invoke(this,
                        new PropertyChangingEventArgs(propertyName));
                }
            }
        }

        /// <summary>
        /// Override this method to change the default logic for determining 
        /// if the event handler should be serialized
        /// </summary>
        /// <param name="value">the event handler to review</param>
        /// <returns></returns>
        protected virtual bool ShouldHandlerSerialize(PropertyChangingEventHandler value)
        {
            return value.Method.IsPublic &&
                   value.Method.DeclaringType != null &&
                   (value.Method.DeclaringType.IsSerializable || value.Method.IsStatic);
        }

        #endregion

        /// <summary>
        /// apply with using pattern to temporally disable binding events
        /// </summary>
        private sealed class SuspendBindingsInt : IDisposable
        {
            private bool disposedValue;
            private BindableBase _forDomainObject;

            /// <summary>
            /// apply with using pattern to temporally disable binding events 
            /// </summary>
            /// <param name="forDomainObject">a domain object to disable the binding events for</param>
            public SuspendBindingsInt(BindableBase forDomainObject)
            {
                _forDomainObject = forDomainObject ?? throw new ArgumentNullException(nameof(forDomainObject));
                forDomainObject._suspendBinding = true;
            }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _forDomainObject._suspendBinding = false;
                    }

                    disposedValue = true;
                }
            }

            
            void IDisposable.Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
            }
        }
    }
}

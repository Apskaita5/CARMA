using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A class that manages a child field binding and events.
    internal sealed class ChildFieldManager<TClass>
        where TClass : DomainObject<TClass>
    {
        [NonSerialized]
        private readonly ChildFieldInfo<TClass> _info;
        [NonSerialized]
        private readonly ChildEventContainer _eventContainer = new ChildEventContainer();


        /// <summary>
        /// Initializes this object with the provided ChildFieldInfo.
        /// If no argument is provided, throws an ArgumentNullException.
        /// </summary>
        /// <param name="info">The ChildFieldInfo used to initialize this ChildFieldManager. It cannot be null.</param>
        public ChildFieldManager(ChildFieldInfo<TClass> info)
        {
            _info = info ?? throw new ArgumentNullException(nameof(info));
        }


        /// <summary>
        /// Gets the name of the property that exposes the child field.
        /// </summary>
        public string PropertyName => _info.PropertyName;

        /// <summary>
        /// Gets the type of the field.
        public Type FieldType => _info.FieldType;

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string FieldName => _info.FieldName;

        /// <summary>
        /// Gets a value indicating whether the child value is <see cref="IBindable"/>.
        /// </summary>
        public bool IsBindable => _info.IsBindable;

        /// <summary>
        /// Gets a value indicating whether the child value is <see cref="ITrackState"/>.
        /// </summary>
        public bool IsStateful => _info.IsStateful;

        /// <summary>
        /// Gets a value indicating whether the child value implements the INotifyPropertyChanged interface.
        /// </summary>
        public bool IsINotifyPropertyChanged => _info.IsINotifyPropertyChanged;

        /// <summary>
        /// Gets a value indicating whether the child value implements the IBindingList interface.
        /// </summary>
        public bool IsIBindingList => _info.IsIBindingList;

        /// <summary>
        /// Gets a value indicating whether the child value implements the INotifyCollectionChanged interface.
        /// </summary>
        public bool IsINotifyCollectionChanged => _info.IsINotifyCollectionChanged;

        /// <summary>
        /// Gets a value indicating whether the child value is <see cref="INotifyChildChanged"/>.
        /// </summary>
        public bool IsINotifyChildChanged => _info.IsINotifyChildChanged;

        /// <summary>
        /// Gets a value indicating whether the child field has any subscribed events.
        /// </summary>
        public bool WithEvents => _eventContainer.WithEvents;


        /// <summary>
        /// Adds child events for a given parent object of type TClass.
        public void AddEventsFor(TClass parent)
        {
            if (null == parent) throw new ArgumentNullException(nameof(parent));

            _eventContainer.AddEventsFor(parent, _info.Getter(parent), _info.PropertyName);
        }

        /// <summary>
        /// Removes all events related to the child field value for the parent object.
        /// </summary>
        /// <param name="parent">The parent object of the events that need removing.</param>
        public void RemoveEventsFor(TClass parent)
        {
            if (null == parent) throw new ArgumentNullException(nameof(parent));

            _eventContainer.RemoveEventsFor(parent, _info.Getter(parent), _info.PropertyName);
        }

        /// <summary>
        /// Updates the binding options for the child value when the parent object changes these options.
        /// </summary>
        /// <param name="parent">The parent object</param>
        public void UpdateBindingOptions(TClass parent)
        {
            if (null == parent) throw new ArgumentNullException(nameof(parent));

            if (!typeof(IBindable).IsAssignableFrom(typeof(TClass)) ||
                !typeof(IBindableChildInt).IsAssignableFrom(FieldType)) return;

            var child = _info.Getter(parent) as IBindableChildInt;
            if (ReferenceEquals(child, null)) return;
            child.UpdateBindingOptions(parent);
        }

        /// <summary>
        /// Retrieves the stateful child of the provided parent object if existent and if the object is stateful.
        /// </summary>
        /// <param name="parent">The parent object from which the stateful child should be retrieved.</param>
        /// <returns>
        /// An ITrackState object representing the stateful child of the given parent.
        /// If the parent object is null, an ArgumentNullException is thrown.
        /// If the child value is not stateful, null is returned.
        /// </returns>
        public ITrackState GetChildAsStateful(TClass parent)
        {
            if (null == parent) throw new ArgumentNullException(nameof(parent));

            if (!IsStateful) return null;

            return _info.Getter(parent) as ITrackState;
        }

        #region Container

        /// <summary>
        /// Container for event delegates.
        private class ChildEventContainer
        {
            [NonSerialized]
            private PropertyChangedEventHandler _propertyChangedDelegate;
            [NonSerialized]
            private ListChangedEventHandler _listChangedDelegate;
            [NonSerialized]
            private NotifyCollectionChangedEventHandler _collectionChangedDelegate;
            [NonSerialized]
            private EventHandler<ChildChangedEventArgs> _childChangedDelegate;


            /// <summary>
            /// Gets a value indicating whether events are associated with this instance.
            /// </summary>
            public bool WithEvents { get; private set; }


            /// <summary>
            /// Adds relevant event handlers for the specified child object of the given parent object based on their types.
            /// If the child object implements relevant interfaces such as IBindableChildInt, IChildInternal, INotifyPropertyChanged,
            /// IBindingList, INotifyCollectionChanged, or INotifyChildChanged, corresponding event handlers will be added.
            /// </summary>
            /// <param name="parent">The parent object of type TClass.</param>
            /// <param name="child">The child object of any generic type.</param>
            /// <param name="PropertyName">The name of the property that represents the child object in the parent object.</param>
            /// <exception cref="ArgumentNullException">Thrown when the passed parent object is null.</exception>
            public void AddEventsFor(TClass parent, Object child, string PropertyName)
            {
                if (null == parent) throw new ArgumentNullException(nameof(parent));

                WithEvents = false;

                if (ReferenceEquals(child, null)) return;

                if (parent is IBindable && child is IBindableChildInt bb)
                {
                    bb.UpdateBindingOptions(parent);
                }

                if (child is IChildInternal childInt)
                {
                    childInt.SetParent(parent, PropertyName);
                }

                if (child is INotifyPropertyChanged pc)
                {
                    _propertyChangedDelegate = (o, e) =>
                        parent.ChildHasChanged(PropertyName, o, CreateChildChangedEventArgs(o, e));
                    pc.PropertyChanged += _propertyChangedDelegate;
                    WithEvents = true;
                }

                if (child is IBindingList bl)
                {
                    _listChangedDelegate = (o, e) =>
                        parent.ChildHasChanged(PropertyName, o, CreateChildChangedEventArgs(o, null, e));
                    bl.ListChanged += _listChangedDelegate;
                    WithEvents = true;
                }
                else if (child is INotifyCollectionChanged ncc)
                {
                    _collectionChangedDelegate = (o, e) =>
                        parent.ChildHasChanged(PropertyName, o, CreateChildChangedEventArgs(o, null, e));
                    ncc.CollectionChanged += _collectionChangedDelegate;
                    WithEvents = true;
                }

                if (child is INotifyChildChanged nchc)
                {
                    _childChangedDelegate = (o, e) =>
                        parent.ChildHasChanged(PropertyName, o, e);
                    nchc.ChildChanged += _childChangedDelegate;
                    WithEvents = true;
                }
            }

            /// <summary>
            /// Removes the events associated with a given child object from the parent.
            /// This method particularly targets the PropertyChanged, ListChanged, CollectionChanged, and ChildChanged events.
            /// </summary>
            /// <param name="parent">The parent instance where the events are to be removed from. It cannot be null.</param>
            /// <param name="child">The child object associated with the events to be removed. If it's null, the method will return immediately.</param>
            /// <param name="PropertyName">The property name related to the event.</param>
            public void RemoveEventsFor(TClass parent, Object child, string PropertyName)
            {
                if (null == parent) throw new ArgumentNullException(nameof(parent));

                WithEvents = false;

                if (ReferenceEquals(child, null)) return;

                if (child is IChildInternal childInt)
                {
                    childInt.SetParent(null, PropertyName);
                }

                if (null != _propertyChangedDelegate && child is INotifyPropertyChanged pc)
                {
                    pc.PropertyChanged -= _propertyChangedDelegate;
                    _propertyChangedDelegate = null;
                }

                if (null != _listChangedDelegate && child is IBindingList bl)
                {
                    bl.ListChanged -= _listChangedDelegate;
                    _listChangedDelegate = null;
                }
                else if (null != _collectionChangedDelegate && child is INotifyCollectionChanged ncc)
                {
                    ncc.CollectionChanged -= _collectionChangedDelegate;
                    _collectionChangedDelegate = null;
                }

                if (null != _childChangedDelegate && child is INotifyChildChanged nchc)
                {
                    nchc.ChildChanged -= _childChangedDelegate;
                    _childChangedDelegate = null;
                }
            }


            private static ChildChangedEventArgs CreateChildChangedEventArgs(object childObject,
                PropertyChangedEventArgs propertyArgs)
            {
                return new ChildChangedEventArgs(childObject, propertyArgs);
            }

            private static ChildChangedEventArgs CreateChildChangedEventArgs(object childObject,
                PropertyChangedEventArgs propertyArgs, ListChangedEventArgs listArgs)
            {
                return new ChildChangedEventArgs(childObject, propertyArgs, listArgs);
            }

            private static ChildChangedEventArgs CreateChildChangedEventArgs(object childObject,
                PropertyChangedEventArgs propertyArgs, NotifyCollectionChangedEventArgs listArgs)
            {
                return new ChildChangedEventArgs(childObject, propertyArgs, listArgs);
            }
        }

        #endregion
    }
}
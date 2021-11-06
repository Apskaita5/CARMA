using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;
using A5Soft.CARMA.Domain.Rules;
using A5Soft.CARMA.Domain.Rules.DataAnnotations;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// This is the base class from which most business collections or lists will be derived.
    /// </summary>
    /// <typeparam name="TChild">Type of item contained in list.</typeparam>
    [Serializable]
    public class DomainBindingList<TChild> : BindingList<TChild>, 
        INotifyChildChanged, ITrackState, IChildInternal, IBindable, INotifyCollectionChanged
        where TChild : class, ITrackState
    {

        #region Constructors

        /// <summary>
        /// Creates a parent instance of the object.
        /// </summary>
        /// <param name="validationEngineProvider">Validation engine to use for child items.</param>
        public DomainBindingList(IValidationEngineProvider validationEngineProvider)
        {
            Initialize();
            this.AllowNew = true;
            _validationEngineProvider = validationEngineProvider 
                ?? throw new ArgumentNullException(nameof(validationEngineProvider));
        }

        /// <summary>
        /// Creates an instance of the object as a child of parent domain object.
        /// </summary>
        /// <param name="parent">a parent of the list</param>
        /// <param name="validationEngineProvider">Validation engine to use for child items.</param>
        public DomainBindingList(IDomainObject parent, IValidationEngineProvider validationEngineProvider)
        {
            Initialize();
            this.AllowNew = true;
            _validationEngineProvider = validationEngineProvider
                ?? throw new ArgumentNullException(nameof(validationEngineProvider));
            SetParent(parent);
        }

        /// <summary>
        /// Creates a copy of the list.
        /// </summary>
        /// <param name="listToCopy">a list to copy</param>
        /// <param name="newParent">a (new) parent for the new list</param>
        /// <param name="copyChildMethod">a method to copy children</param>
        public DomainBindingList(DomainBindingList<TChild> listToCopy, IDomainObject newParent,
            Func<TChild, TChild> copyChildMethod)
        {
            if (null == listToCopy) throw new ArgumentNullException(nameof(listToCopy));
            if (null == copyChildMethod) throw new ArgumentNullException(nameof(copyChildMethod));

            Initialize();

            this.AllowNew = true;
            _validationEngineProvider = listToCopy._validationEngineProvider;
            SetParent(newParent);

            AddRange(listToCopy.Select(c => copyChildMethod(c)), false);

            this.AllowEdit = listToCopy.AllowEdit;
            this.AllowNew = listToCopy.AllowNew;
            this.AllowRemove = listToCopy.AllowRemove;
            this._bindingMode = listToCopy._bindingMode;
            this._notifyPropertyChangedEnabled = listToCopy._notifyPropertyChangedEnabled;
            this._notifyPropertyChangingEnabled = listToCopy._notifyPropertyChangingEnabled;
            this.ChildFactory = listToCopy.ChildFactory;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Override this method to set up event handlers so user
        /// code in a partial class can respond to events raised by
        /// generated code.
        /// </summary>
        protected virtual void Initialize()
        { /* allows subclass to initialize events before any other activity occurs */ }

        #endregion

        #region ITrackState

        [NonSerialized]
        private IValidationEngineProvider _validationEngineProvider;


        protected IValidationEngineProvider ValidationEngineProvider {
            get
            {
                if (_validationEngineProvider.IsNull())
                {
                    _validationEngineProvider = new DefaultValidationEngineProvider(
                        new DefaultMetadataProvider());
                }

                return _validationEngineProvider;
            }
        }

        /// <inheritdoc cref="ITrackState.SetValidationEngine" />
        public void SetValidationEngine(IValidationEngineProvider validationEngineProvider)
        {
            _validationEngineProvider = validationEngineProvider;
            foreach (var statefulChild in this)
            {
                statefulChild?.SetValidationEngine(validationEngineProvider);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this list or any child item has been changed.
        /// </summary>
        bool ITrackState.IsSelfDirty
        {
            get { return IsDirty; }
        }

        /// <summary>
        /// Gets a value indicating whether this list or any child item has been changed.
        /// </summary>
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public bool IsDirty
        {
            get
            {
                return DeletedList.Count > 0 || this.Any(c => c.IsDirty);
            }
        }

        /// <summary>
        /// Returns true if any item has been added to the list or deleted from the list.
        /// </summary>
        bool ITrackState.SelfContainsNewData
        {
            get { return ContainsNewData; }
        }

        /// <summary>
        /// Returns true if any item has been added to the list or deleted from the list.
        /// </summary>
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool ContainsNewData {
            get
            {
                // adding a new child even without data is enough to consider the collection having some new data
                return (DeletedList.Count > 0 || this.Any(c => c.IsDirty));
            }
        }

        bool ITrackState.IsSelfValid
        {
            get { return IsValid; }
        }

        /// <summary>
        /// Gets a value indicating whether all of the child items are in a valid state
        /// (have no broken validation rules).
        /// </summary>
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool IsValid
        {
            get
            {
                return !this.Any(c => !c.IsValid);
            }
        }

        bool ITrackState.HasSelfWarnings
        {
            get { return HasWarnings; }
        }

        /// <summary>
        /// Returns true if any child item has any warning level broken rules.
        /// </summary>
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool HasWarnings
        {
            get
            {
                return this.Any(c => c.HasWarnings);
            }
        }
                       
        bool ITrackState.IsDeleted
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if this list is both dirty and valid.
        /// </summary>
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool IsSavable
        {
            get
            {
                return (IsDirty && IsValid);
            }
        }

        string[] ITrackState.LockedProperties
            => new string[] { };


        /// <summary>
        /// Forces check of all the business rules for all the child items in the list.
        /// </summary>
        /// <param name="checkRulesForChildren">whether to invoke CheckRules method
        /// on children of the items in the list</param>
        public void CheckRules(bool checkRulesForChildren = true)
        {
            foreach (var child in this)
            {
                child.CheckRules(checkRulesForChildren);
            }
        }

        bool ITrackState.CanWriteProperty(string propertyName)
        {
            return true;
        }

        /// <inheritdoc cref="ITrackState.GetBrokenRulesTree" />
        public BrokenRulesTreeNode GetBrokenRulesTree(bool useInstanceDescription = false)
        {
            var result = new BrokenRulesTreeNode(this.ToString(), new BrokenRule[]{});

            foreach (var statefulChild in this)
            {
                result.AddBrokenRulesForChild(statefulChild.GetBrokenRulesTree(true));
            }

            return result;
        }

        #endregion

        #region IBindable

        private bool _notifyPropertyChangedEnabled = false;
        private bool _notifyPropertyChangingEnabled = false;
        private BindingMode _bindingMode = BindingMode.WinForms;


        /// <summary>
        /// Enables or disables INotifyPropertyChanged interface for the child instances.
        /// </summary>
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
                    // don't disable for children as it will affect internal functions
                }
            }
        }

        /// <summary>
        /// Enables or disables INotifyPropertyChanging interface for the child instances.
        /// </summary>
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
                    if (typeof(IBindable).IsAssignableFrom(typeof(TChild)))
                    {
                        foreach (IBindable child in this)
                        {
                            child.NotifyPropertyChangingEnabled = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets of sets a binding mode to adapt to a particular GUI framework.
        /// E.g. for WinForms binding schema there should be only one PropertyChanged event
        /// raised per range of changed properties, while for WPF binding schema an event per
        /// each property in range should be raised.
        /// </summary>
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
                    if (typeof(IBindable).IsAssignableFrom(typeof(TChild)))
                    {
                        foreach (IBindable child in this)
                        {
                            child.BindingMode = value;
                        }
                    }
                }
            }
        }
          
        #endregion

        #region Delete and Undelete child

        private List<TChild> _deletedList;

        /// <summary>
        /// A collection containing all child objects marked for deletion.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<TChild> DeletedList
        {
            get
            {
                if (_deletedList == null)
                    _deletedList = new List<TChild>();
                return _deletedList;
            }
        }

        private void DeleteChild(TChild child)
        {
            // add it to the deleted collection for storage
            DeletedList.Add(child);
        }
             
        /// <summary>
        /// Returns true if the internal deleted list contains the specified child object.
        /// </summary>
        /// <param name="item">Child object to check.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool ContainsDeleted(TChild item)
        {
            return DeletedList.Contains(item);
        }

        #endregion

        #region Insert, Remove, Clear

        /// <summary>
        /// Set a child factory in order to enable dynamic adding of new items (<see cref="AddNewCore"/> implementation by injection).
        /// </summary>
        public IChildFactory<TChild> ChildFactory { get; set; } =
            DefaultChildFactory<TChild>.NewOrNull();

        protected override object AddNewCore()
        {
            var item = CreateNewChild();
            Add(item);
            return item;
        }

        /// <summary>
        /// Override this method to create a new child instance that is added to the collection. 
        /// </summary>
        protected virtual TChild CreateNewChild()
        {
            if (null != ChildFactory) return ChildFactory.CreateNew(_parent, _validationEngineProvider);
            throw new NotSupportedException(
                $"AddNewCore is not supported by {this.GetType().FullName} (CreateNewChild not implemented, ChildFactory not set)");
        }

        [NonSerialized()]
        private EventHandler<RemovingItemEventArgs> _nonSerializableHandlers;
        private EventHandler<RemovingItemEventArgs> _serializableHandlers;

        /// <summary>
        /// Implements a serialization-safe RemovingItem event.
        /// </summary>
        public event EventHandler<RemovingItemEventArgs> RemovingItem
        {
            add
            {
                if (value.Method.IsPublic &&
                   (value.Method.DeclaringType.IsSerializable ||
                    value.Method.IsStatic))
                    _serializableHandlers = (EventHandler<RemovingItemEventArgs>)
                      System.Delegate.Combine(_serializableHandlers, value);
                else
                    _nonSerializableHandlers = (EventHandler<RemovingItemEventArgs>)
                      System.Delegate.Combine(_nonSerializableHandlers, value);
            }
            remove
            {
                if (value.Method.IsPublic &&
                   (value.Method.DeclaringType.IsSerializable ||
                    value.Method.IsStatic))
                    _serializableHandlers = (EventHandler<RemovingItemEventArgs>)
                      System.Delegate.Remove(_serializableHandlers, value);
                else
                    _nonSerializableHandlers = (EventHandler<RemovingItemEventArgs>)
                      System.Delegate.Remove(_nonSerializableHandlers, value);
            }
        }

        /// <summary>
        /// Raise the RemovingItem event.
        /// </summary>
        /// <param name="removedItem">A reference to the item that is being removed.
        /// </param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void OnRemovingItem(TChild removedItem)
        {
            _nonSerializableHandlers?.Invoke(this, new RemovingItemEventArgs(removedItem));
            _serializableHandlers?.Invoke(this, new RemovingItemEventArgs(removedItem));
        }

        [NonSerialized()]
        private NotifyCollectionChangedEventHandler _nonSerializableCollectionHandlers;
        private NotifyCollectionChangedEventHandler _serializableCollectionHandlers;

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                if (value.Method.IsPublic &&
                    (value.Method.DeclaringType.IsSerializable ||
                        value.Method.IsStatic))
                    _serializableCollectionHandlers = (NotifyCollectionChangedEventHandler)
                        System.Delegate.Combine(_serializableCollectionHandlers, value);
                else
                    _nonSerializableCollectionHandlers = (NotifyCollectionChangedEventHandler)
                        System.Delegate.Combine(_nonSerializableCollectionHandlers, value);
            }
            remove
            {
                if (value.Method.IsPublic &&
                    (value.Method.DeclaringType.IsSerializable ||
                        value.Method.IsStatic))
                    _serializableCollectionHandlers = (NotifyCollectionChangedEventHandler)
                        System.Delegate.Remove(_serializableCollectionHandlers, value);
                else
                    _nonSerializableCollectionHandlers = (NotifyCollectionChangedEventHandler)
                        System.Delegate.Remove(_nonSerializableCollectionHandlers, value);
            }
        }

        /// <summary>
        /// Raise the CollectionChanged event.
        /// </summary>
        /// <param name="args">event parameters</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (_bindingMode == BindingMode.WinForms) return;
            _nonSerializableCollectionHandlers?.Invoke(this, args);
            _serializableCollectionHandlers?.Invoke(this, args);
        }


        /// <summary>
        /// Remove the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.
        /// </param>
        protected override void RemoveItem(int index)
        {
            // cannot validate operation so hold a reference and proceed to base method that might throw
            TChild child = this[index];

            base.RemoveItem(index);

            OnRemovingItem(child);
            OnRemoveEventHooks(child);

            // the child shouldn't be completely removed,
            // so copy it to the deleted list
            DeleteChild(child);
            if (child is IChildInternal childInternal) childInternal.SetParent(null);

            if (RaiseListChangedEvents)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, child, index));
            }
        }

        /// <summary>
        /// Clears the collection, moving all active items to the deleted list.
        /// </summary>
        protected override void ClearItems()
        {
            if (!this.AllowRemove) throw new InvalidOperationException("AllowRemove property is set to false, cannot clear items.");

            foreach (var child in this)
            {
                OnRemovingItem(child);
                OnRemoveEventHooks(child);
                
                // the child shouldn't be completely removed,
                // so copy it to the deleted list
                DeleteChild(child);

                if (child is IChildInternal childInternal) childInternal.SetParent(null);
            }

            base.ClearItems();
            
            if (RaiseListChangedEvents) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Add a range of items to the list.
        /// </summary>
        /// <param name="range">List of items to add.</param>
        /// <param name="raiseResetEvent">whether to raise ListChangedType.Reset event for
        /// the entire range instead of ListChangedType.ItemAdded events for each item added.</param>
        public void AddRange(IEnumerable<TChild> range, bool raiseResetEvent = false)
        {
            if (raiseResetEvent)
            {
                var newIndex = this.Count;
                using (SuppressListChangedEvents)
                {
                    foreach (var element in range)
                        this.Add(element);  // Add invokes InsertItem
                }

                if (RaiseListChangedEvents)
                {
                    OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, newIndex));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, range));
                }
            }
            else
            {
                foreach (var element in range)
                    this.Add(element);  // Add invokes InsertItem
            }
        }
                
        /// <summary>
        /// Invoked when an item is inserted into the list.
        /// </summary>
        /// <param name="index">Index of new item.</param>
        /// <param name="item">Reference to new item.</param>
        protected override void InsertItem(int index, TChild item)
        {
            base.InsertItem(index, item);

            OnAddEventHooks(item);
            if (item is IChildInternal ci) ci.SetParent(this);

            if (RaiseListChangedEvents) 
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Replaces the item at the specified index with
        /// the specified item, first moving the original
        /// item to the deleted list.
        /// </summary>
        /// <param name="index">The zero-based index of the item to replace.</param>
        /// <param name="item">
        /// The new value for the item at the specified index. 
        /// The value can be null for reference types.
        /// </param>
        /// <remarks></remarks>
        protected override void SetItem(int index, TChild item)
        {
            if (ReferenceEquals(this[index], item)) return;

            var child = this[index];
            if (!child.IsNull())
            {
                if (child is IChildInternal ciOld) ciOld.SetParent(null);
                OnRemoveEventHooks(child);
                DeleteChild(child);
            }

            if (!item.IsNull())
            {
                OnAddEventHooks(item);
                if (item is IChildInternal ciNew) ciNew.SetParent(this);
            }

            base.SetItem(index, item);

            if (RaiseListChangedEvents)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace, item, child, index));
            }
        }

        /// <summary>
        /// merges children data from an untrusted source into the list
        /// </summary>
        /// <typeparam name="TIChild">type of child item domain interface</typeparam>
        /// <param name="childrenByInterface">children data from an untrusted source</param>
        /// <param name="mergeChild">method to merge untrusted data into a child item</param>
        public virtual void Merge<TIChild>(IList<TIChild> childrenByInterface, Action<TChild, TIChild> mergeChild)
            where TIChild : IDomainEntity<TChild>
        {
            if (null == childrenByInterface) throw new ArgumentNullException(nameof(childrenByInterface));
            if (null == mergeChild) throw new ArgumentNullException(nameof(mergeChild));
            if (!typeof(IDomainEntity<TChild>).IsAssignableFrom(typeof(TChild))) throw new InvalidOperationException(
                $"Can only merge domain object lists for children that implement IDomainEntity while {typeof(TChild).FullName} does not.");

            bool found;

            for (int i = this.Count-1; i > -1; i--)
            {
                found = false;

                foreach (var iChild in childrenByInterface)
                {
                    if (((IDomainEntity<TChild>)this[i]).Id == iChild.Id)
                    {
                        mergeChild(this[i], iChild);
                        found = true;
                        break;
                    }
                }

                if (!found && this.AllowRemove)
                {
                    RemoveAt(i);
                }
            }

            if (this.AllowNew)
            {
                foreach (var iChild in childrenByInterface)
                {
                    found = false;
                    foreach (IDomainEntity<TChild> child in this)
                    {
                        if (child.Id == iChild.Id)
                        {
                           found = true;
                           break;
                        }
                    }

                    if (!found)
                    {
                        var newItem = this.AddNew();
                        mergeChild(newItem, iChild);
                    }
                }
            }
        }


        /// <summary>
        /// Method invoked when events are hooked for a child
        /// object.
        /// </summary>
        /// <param name="item">Reference to child object.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnAddEventHooks(TChild item)
        {
            if (item is INotifyPropertyChanged npc && !npc.IsNull())
                npc.PropertyChanged += Child_PropertyChanged;

            if (item is INotifyChildChanged ncc && !ncc.IsNull())
                ncc.ChildChanged += Child_Changed;

            if (item is IBindable bindableItem)
            {
                bindableItem.NotifyPropertyChangingEnabled = _notifyPropertyChangingEnabled;
                bindableItem.BindingMode = _bindingMode;
                bindableItem.NotifyPropertyChangedEnabled = true;
            }
        }

        /// <summary>
        /// Method invoked when events are unhooked for a child object.
        /// </summary>
        /// <param name="item">Reference to child object.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnRemoveEventHooks(TChild item)
        {
            if (item is INotifyPropertyChanged npc && !npc.IsNull())
                npc.PropertyChanged -= Child_PropertyChanged;

            if (item is INotifyChildChanged ncc && !ncc.IsNull())
                ncc.ChildChanged -= Child_Changed;
        }

        #endregion

        #region Serialization Notification

        [NonSerialized]
        private bool _deserialized = false;

        /// <summary>
        /// This method is called on a newly deserialized object after deserialization is complete.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDeserialized()
        { }
             
        [System.Runtime.Serialization.OnDeserialized]
        private void OnDeserializedHandler(System.Runtime.Serialization.StreamingContext context)
        {
            if (typeof(IChildInternal).IsAssignableFrom(typeof(TChild)))
            {
                foreach (var child in this)
                    ((IChildInternal)child)?.SetParent(this);
            }

            foreach (var item in this)
                OnAddEventHooks(item);

            _deserialized = true;

            OnDeserialized();
        }

        #endregion

        #region Cascade child events

        [NonSerialized]
        private EventHandler<ChildChangedEventArgs> _childChangedHandlers;

        /// <summary>
        /// Event raised when a child object has been changed.
        /// </summary>
        public event EventHandler<ChildChangedEventArgs> ChildChanged
        {
            add
            {
                _childChangedHandlers = (EventHandler<ChildChangedEventArgs>)
                  System.Delegate.Combine(_childChangedHandlers, value);
            }
            remove
            {
                _childChangedHandlers = (EventHandler<ChildChangedEventArgs>)
                  System.Delegate.Remove(_childChangedHandlers, value);
            }
        }

        /// <summary>
        /// Raises the ChildChanged event, indicating that a child object has been changed.
        /// </summary>
        /// <param name="e">ChildChangedEventArgs object. </param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnChildChanged(ChildChangedEventArgs e)
        {
            _childChangedHandlers?.Invoke(this, e);
        }

        /// <summary>
        /// Creates a ChildChangedEventArgs and raises the event.
        /// </summary>
        private void RaiseChildChanged(object childObject, PropertyChangedEventArgs propertyArgs, 
            ListChangedEventArgs listArgs)
        {
            ChildChangedEventArgs args = new ChildChangedEventArgs(childObject, propertyArgs, listArgs);
            OnChildChanged(args);
        }


        /// <summary>
        /// Handles any PropertyChanged event from a child object and echoes it up as a ChildChanged event.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Property changed args.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_deserialized && RaiseListChangedEvents && !e.IsNull())
            {
                for (int index = 0; index < Count; index++)
                {
                    if (ReferenceEquals(this[index], sender))
                    {
                        OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
                        break;
                    }
                }
            }
            RaiseChildChanged(sender, e, null);
        }

        /// <summary>
        /// Handles any ChildChanged event from a child object and echoes it up as a ChildChanged event.
        /// </summary>
        private void Child_Changed(object sender, ChildChangedEventArgs e)
        {
            RaiseChildChanged(e.ChildObject, e.PropertyChangedArgs, e.ListChangedArgs);
        }

        #endregion

        #region Parent/Child link

        [NonSerialized]
        private IDomainObject _parent;
        private bool _isChild;


        /// <inheritdoc cref="IChild.Parent" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [IgnorePropertyMetadata]
        public IDomainObject Parent
            => _parent;

        /// <inheritdoc cref="ITrackState.IsChild" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public bool IsChild
            => _isChild;


        /// <summary>
        /// Used by BusinessListBase as a child object is created to tell the child object about its parent.
        /// </summary>
        /// <param name="parent">A reference to the parent collection object.</param>
        protected virtual void SetParent(IDomainObject parent)
        {
            _parent = parent;
            if (!parent.IsNull()) MarkAsChild();
        }

        void IChildInternal.SetParent(IDomainObject parent)
        {
            SetParent(parent);
        }

        /// <summary>
        /// Marks the object as being a child object.
        /// </summary>
        protected void MarkAsChild()
        {
            _isChild = true;
        }

        #endregion


        /// <summary>
        /// Use this object to suppress ListChangedEvents for an entire code block.
        /// May be nested in multiple levels for the same object.
        /// </summary>
        public IDisposable SuppressListChangedEvents
        {
            get { return new SuppressListChangedEventsClass<TChild>(this); }
        }

        /// <summary>
        /// Handles the suppressing of raising ChangedEvents when altering the content of an ObservableBindingList.
        /// Will be instantiated by a factory property on the ObservableBindingList implementation.
        /// </summary>
        /// <typeparam name="TC">The type of the C.</typeparam>
        class SuppressListChangedEventsClass<TC> : IDisposable
        {
            private readonly BindingList<TC> _businessObject;
            private readonly bool _initialRaiseListChangedEvents;

            public SuppressListChangedEventsClass(BindingList<TC> businessObject)
            {
                this._businessObject = businessObject;
                _initialRaiseListChangedEvents = businessObject.RaiseListChangedEvents;
                businessObject.RaiseListChangedEvents = false;
            }

            public void Dispose()
            {
                _businessObject.RaiseListChangedEvents = _initialRaiseListChangedEvents;
            }
        }

    }
}

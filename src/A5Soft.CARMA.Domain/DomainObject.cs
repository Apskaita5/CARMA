using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// a base class for all domain objects that needs to track their state
    /// </summary>
    /// <typeparam name="T">type of the domain entity implementation</typeparam>
    /// <remarks>inherit from this class for entities that have no identity
    /// (only one instance per domain, e.g. some common settings) and no auditing functionality</remarks>
    [Serializable]
    public abstract class DomainObject<T> 
        : BindableBase, ITrackState, IChildInternal, INotifyChildChanged, IDataErrorInfo
        where T : DomainObject<T>
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        protected DomainObject(IValidationEngineProvider validationEngineProvider)
        {
            if (validationEngineProvider.IsNull()) throw new ArgumentNullException(nameof(validationEngineProvider));

            Initialize();
            _brokenRules = new BrokenRulesManager<T>((T)this, validationEngineProvider);
        }

        /// <summary>
        /// Creates a copy of the domain object.
        /// </summary>
        /// <param name="objectToCopy">a domain object to copy</param>
        protected DomainObject(T objectToCopy)
        {
            if (objectToCopy.IsNull()) throw new ArgumentNullException(nameof(objectToCopy));

            Initialize();
            _brokenRules = objectToCopy._brokenRules.GetCopy((T)this);
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Override this method to set up event handlers so user code in a partial class
        /// can respond to events raised by generated code.
        /// If child objects are initialized, invoke RegisterChildValue method.
        /// </summary>
        protected virtual void Initialize()
        { /* allows subclass to initialize events before any other activity occurs */ }
               
        #endregion
        
        #region IsDeleted, IsDirty, IsValid, IsSavable, ContainsNewData, HasWarnings, BrokenRules

        // keep track of whether we are deleted or dirty (isNew is defined by identity)
        private bool _isDeleted;
        private bool _containsNewData;
        private bool _isDirty = true;
        
        [NonSerialized]
        private BrokenRulesManager<T> _brokenRules;
        [NonSerialized]
        private bool _suspendValidation = false;


        private BrokenRulesManager<T> RulesManager {
            get
            {
                if (_brokenRules.IsNull())
                {
                    _brokenRules = new BrokenRulesManager<T>((T)this, null);
                }

                return _brokenRules;
            }
        }

        /// <inheritdoc cref="ITrackState.SetValidationEngine" />
        public void SetValidationEngine(IValidationEngineProvider validationEngineProvider)
        {
            _brokenRules = new BrokenRulesManager<T>((T)this, validationEngineProvider);
            foreach (var statefulChild in GetStatefulChildren())
            {
                statefulChild.SetValidationEngine(validationEngineProvider);
            }
        }

        /// <inheritdoc cref="ITrackState.IsDeleted" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public bool IsDeleted 
            => _isDeleted;

        /// <inheritdoc cref="ITrackState.IsDirty" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool IsDirty
            => IsSelfDirty || AnyStatefulChildren(c => c.IsDirty);

        /// <inheritdoc cref="ITrackState.IsSelfDirty" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool IsSelfDirty 
            => _isDirty;

        /// <inheritdoc cref="ITrackState.ContainsNewData" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool ContainsNewData
            => SelfContainsNewData || AnyStatefulChildren(c => c.ContainsNewData);

        /// <inheritdoc cref="ITrackState.SelfContainsNewData" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool SelfContainsNewData
            => _containsNewData;


        /// <summary>
        /// Marks the object as being a new object. This also marks the object
        /// as being dirty and ensures that it is not marked for deletion.
        /// </summary>
        /// <remarks>
        /// If you override this method, make sure to call the base
        /// implementation after executing your new code.
        /// </remarks>
        protected virtual void MarkNew()
        {
            _isDeleted = false;
            _isDirty = true;
            _containsNewData = false;
            OnUnknownPropertyChanged();
        }

        /// <summary>
        /// Marks an object for deletion. This also marks the object as being dirty.
        /// </summary>
        /// <remarks>
        /// You should call this method in your business logic in the  case that you want
        /// to have the object deleted when it is saved to the database.
        /// </remarks>
        protected void MarkDeleted()
        {
            _isDeleted = true;
            MarkDirty();
        }

        /// <summary>
        /// Marks an object as being dirty, or changed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You should call this method in your business logic any time
        /// the object's internal data changes. 
        /// </para><para>
        /// Marking an object as dirty does two things. First it ensures  that the object will be properly saved.
        /// Second, it causes to tell Windows Forms data binding that the object's data has changed
        /// so any bound controls will update to reflect the new values.
        /// </para>
        /// </remarks>
        protected void MarkDirty()
        {
            MarkDirty(false);
        }

        /// <summary>
        /// Marks an object as being dirty, or changed.
        /// </summary>
        /// <param name="suppressEvent">
        /// true to supress the PropertyChanged event that is otherwise
        /// raised to indicate that the object's state has changed.
        /// </param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void MarkDirty(bool suppressEvent)
        {
           _isDirty = true;
           _containsNewData = true;
            if (!suppressEvent) OnUnknownPropertyChanged();
        }

        /// <summary>
        /// Performs processing required when a property has changed.
        /// </summary>
        /// <param name="propertyName">Property that has changed.</param>
        /// <remarks>
        /// This method calls CheckRules(propertyName), MarkDirty and  OnPropertyChanged(propertyName).
        /// MarkDirty is called such that no event is raised for IsDirty, so only the specific
        /// property changed event for the current property is raised.
        /// </remarks>
        protected virtual void PropertyHasChanged(string propertyName)
        {
            MarkDirty(true);
            CheckPropertyRules(propertyName);
        }

        /// <summary>
        /// Performs processing required when a range of properties have changed.
        /// </summary>
        /// <param name="propertyNames">Property that has changed.</param>
        /// <remarks>
        /// This method calls MarkDirty once, CheckRules(propertyName) for every property
        /// and OnPropertiesChanged(affectedProps) once for all affected properties.
        /// MarkDirty is called such that no event is raised for IsDirty, so only the specific
        /// property changed event for the current property is raised.
        /// </remarks>
        protected virtual void PropertiesHaveChanged(params string[] propertyNames)
        {
            MarkDirty(true);
            CheckPropertyRules(propertyNames);
        }

        /// <summary>
        /// Check rules for the property and notifies UI of properties that may have changed.
        /// </summary>
        /// <param name="propertyNames">The properties to check the rules for.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void CheckPropertyRules(params string[] propertyNames)
        {
            if (_suspendValidation) return;
            if (null == propertyNames || propertyNames.Length < 1) 
                throw new ArgumentNullException(nameof(propertyNames));

            var affectedProps = new List<string>();
            foreach (var propertyName in propertyNames)
            {
                affectedProps.AddRange(RulesManager.CheckPropertyRules(propertyName));
            }
            OnPropertiesChanged(affectedProps.Distinct().ToArray());
        }

        /// <summary>
        /// Check object rules and notifies UI of properties that may have changed. 
        /// </summary>
        protected virtual void CheckObjectRules()
        {
            RulesManager.CheckObjectRules();

            if (BindingMode == BindingMode.WinForms)
            {
                OnUnknownPropertyChanged();
            }
            else
            {
                OnPropertiesChanged(RulesManager.Metadata.PropertyNames);
            }
        }

        /// <inheritdoc cref="ITrackState.CheckRules" />
        public void CheckRules(bool checkRulesForChildren = true)
        {
            CheckObjectRules();
            if (checkRulesForChildren)
            {
                foreach (var statefulChild in GetStatefulChildren())
                {
                    statefulChild.CheckRules();
                }
            }
        }

        /// <summary>
        /// Forces the object's IsDirty and ContainsNewData flags to false.
        /// </summary>
        /// <remarks>
        /// This method is normally called automatically and is not intended to be called manually.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void MarkClean()
        {
            _isDirty = false;
            _containsNewData = false;
            OnUnknownPropertyChanged();
        }

        /// <inheritdoc cref="ITrackState.IsSavable" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool IsSavable
            => IsDirty && IsValid;

        /// <inheritdoc cref="ITrackState.IsValid" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool IsValid
        {
            get { return IsSelfValid && !AnyStatefulChildren(c => !c.IsValid); }
        }

        /// <inheritdoc cref="ITrackState.IsSelfValid" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool IsSelfValid 
            => RulesManager.ErrorCount < 1;

        /// <inheritdoc cref="ITrackState.HasWarnings" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool HasWarnings
        {
            get { return HasSelfWarnings || AnyStatefulChildren(c => c.HasWarnings); }
        }

        /// <inheritdoc cref="ITrackState.HasSelfWarnings" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual bool HasSelfWarnings
            => RulesManager.WarningCount > 0;

        /// <summary>
        /// Gets a collection of broken rules for the domain entity.
        /// </summary>
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public BrokenRules BrokenRules 
            => RulesManager;

        /// <inheritdoc cref="ITrackState.LockedProperties" />
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public virtual string[] LockedProperties 
            => new string[] { };


        /// <inheritdoc cref="ITrackState.GetBrokenRulesTree" />
        public BrokenRulesTreeNode GetBrokenRulesTree(bool useInstanceDescription = false)
        {
            string description;

            if (useInstanceDescription)
            {
                description = this.ToString();
            }
            else
            {
                // because if it has state (is editable) yet has no identity,
                // than it's a singleton domain entity, i.e. always old
                var isNew = (this as IDomainEntity<T>)?.IsNew ?? false;
                description = isNew
                    ? RulesManager.Metadata.GetDisplayNameForNew()
                    : RulesManager.Metadata.GetDisplayNameForOld();
            }

            var result = new BrokenRulesTreeNode(description, RulesManager.ToBrokenRuleArray());

            foreach (var statefulChild in GetStatefulChildren())
            {
                result.AddBrokenRulesForChild(statefulChild.GetBrokenRulesTree(false));
            }

            return result;
        }

        /// <inheritdoc cref="ITrackState.CanWriteProperty" />
        public virtual bool CanWriteProperty(string propertyName)
        {
            return true;
        }

        /// <summary>
        /// apply with using pattern to temporally disable validation
        /// </summary>
        public IDisposable SuspendValidation() => new SuspendValidationInt<DomainObject<T>>(this);

        #endregion

        #region Delete

        /// <summary>
        /// Marks the object for deletion. The object will be deleted as part of the
        /// next save operation.
        /// </summary>
        /// <remarks>
        /// This method is part of the support for deferred deletion, where an object
        /// can be marked for deletion, but isn't actually deleted until the object
        /// is saved to the database. This method is called by the UI developer to
        /// mark the object for deletion.
        /// </remarks>
        public virtual void Delete()
        {
            if (this.IsChild)
                throw new NotSupportedException("Method Delete is not applicable for child entity.");

            MarkDeleted();
        }

        /// <summary>
        /// Called by a parent object to mark the child for deferred deletion.
        /// </summary>
        internal void DeleteChild()
        {
            if (!this.IsChild)
                throw new NotSupportedException("Method DeleteChild is not applicable for parent entity.");

            MarkDeleted();
        }

        #endregion

        #region IDataErrorInfo

        [IgnorePropertyMetadata]
        string IDataErrorInfo.Error
        {
            get
            {
                if (!IsSelfValid)
                    return RulesManager.ToString(RuleSeverity.Error);
                return string.Empty;
            }
        }

        [IgnorePropertyMetadata]
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!IsSelfValid) return RulesManager
                    .GetFirstBrokenRule(columnName ?? string.Empty)?
                    .Description ?? string.Empty;
                return string.Empty;
            }
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

        /// <inheritdoc cref="IChildInternal.SetParent" />
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

        #region Child bindings

        private readonly List<string> _statefulChildren = new List<string>();
        private readonly List<string> _bindableChildren = new List<string>();
        

        /// <summary>
        /// Child field setter that shall be used to set child object fields (init or update)
        /// of the following types:
        /// 1. BindableBase;
        /// 2. ITrackState;
        /// 3. Any other class type if its state should be tracked by hooking into its events and
        /// it implements INotifyPropertyChanging, NotifyPropertyChanged,  IBindingList or INotifyCollectionChanged.
        /// </summary>
        /// <typeparam name="TValue">a type of the child object</typeparam>
        /// <param name="fieldName">a name of the field that the property value is stored by</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="addEventHooks">whether to hook into the child object events</param>
        protected void SetChildField<TValue>(string fieldName,
            ref TValue oldValue, TValue newValue, bool addEventHooks = true)
            where TValue : class
        {
            if (fieldName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(fieldName));
            
            UnRegisterChildValue(oldValue, fieldName);

            oldValue = newValue;

            RegisterChildValue(oldValue, fieldName, addEventHooks);
        }

        /// <summary>
        /// Registers a new (incl. initial) value of a child field.  
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="childValue">a new (incl. initial) value of a child field</param>
        /// <param name="fieldName">a name of the field that the child value is stored by</param>
        /// <param name="withEvents">whether to hook into the child object events</param>
        protected virtual void RegisterChildValue<TC>(TC childValue, string fieldName, bool withEvents = true) where TC : class
        {
            if (fieldName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(fieldName));

            if (childValue.IsNull()) return;

            if (withEvents) AddEventHooks(childValue, fieldName);

            if (childValue is IChildInternal cin) cin.SetParent(this);

            if (typeof(IBindable).IsAssignableFrom(childValue.GetType()) 
                && !_bindableChildren.Contains(fieldName))
                _bindableChildren.Add(fieldName);

            if (childValue is ITrackState && !_statefulChildren.Contains(fieldName))
                _statefulChildren.Add(fieldName);
        }

        /// <summary>
        /// Unregisters an old value of a child field.
        /// Method shall be invoked before the child value is updated for those fields
        /// that were registered by RegisterChildValue method.  
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="childValue">an old value of a child field (before it is updated)</param>
        /// <param name="fieldName">a name of the field that the child value is stored by</param>
        protected virtual void UnRegisterChildValue<TC>(TC childValue, string fieldName) where TC : class
        {
            if (fieldName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(fieldName));

            if (childValue.IsNull())
            {
                if (_childrenWithEvents.Contains(fieldName)) _childrenWithEvents.Remove(fieldName);
            }
            else
            {
                RemoveEventHooks(childValue, fieldName);
                if (childValue is IChildInternal cin) cin.SetParent(null);
            }

            if (_bindableChildren.Contains(fieldName)) _bindableChildren.Remove(fieldName);
            if (_statefulChildren.Contains(fieldName)) _statefulChildren.Remove(fieldName);
        }

        /// <summary>
        /// Gets an array of children that implements ITrackState and have their value set. 
        /// </summary>
        /// <returns></returns>
        protected ITrackState[] GetStatefulChildren()
        {
            var result = new List<ITrackState>();
            foreach (var statefulChildName in _statefulChildren)
            {
                var child = GetStatefulChild(statefulChildName);
                if (!child.IsNull()) result.Add(child);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets a value indicating that any child, that implements ITrackState, has state defined by the predicate.
        /// </summary>
        /// <param name="predicate">a child state to find</param>
        /// <returns>a value indicating that any child, that implements ITrackState, has state defined by the predicate.</returns>
        protected bool AnyStatefulChildren(Func<ITrackState, bool> predicate)
        {
            foreach (var statefulChildName in _statefulChildren)
            {
                var child = GetStatefulChild(statefulChildName);
                if (!child.IsNull() && predicate.Invoke(child)) return true;
            }

            return false;
        }

        private ITrackState GetStatefulChild(string fieldName)
        {
            var childField = typeof(T).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (null == childField)
            {
                var propField = typeof(T).GetProperty(fieldName);
                if (null != propField)
                {
                    if (propField.GetValue(this) is ITrackState result) return result;
                }
            }
            else if (childField.GetValue(this) is ITrackState result) return result;

            return null;
        }

        #region Bubbling event Hooks

        [NonSerialized]
        private readonly Dictionary<string, PropertyChangedEventHandler> _propertyChangedDelegates
            = new Dictionary<string, PropertyChangedEventHandler>();
        [NonSerialized]
        private readonly Dictionary<string, ListChangedEventHandler> _listChangedDelegates
            = new Dictionary<string, ListChangedEventHandler>();
        [NonSerialized]
        private readonly Dictionary<string, NotifyCollectionChangedEventHandler> _collectionChangedDelegates
            = new Dictionary<string, NotifyCollectionChangedEventHandler>();
        [NonSerialized]
        private readonly Dictionary<string, EventHandler<ChildChangedEventArgs>> _childChangedDelegates
            = new Dictionary<string, EventHandler<ChildChangedEventArgs>>();
        private readonly List<string> _childrenWithEvents = new List<string>();


        /// <summary>
        /// For internal use.
        /// </summary>
        /// <param name="child">Child object.</param>
        /// <param name="fieldName">a name of a (private) field that manages child value</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void AddEventHooks<TC>(TC child, string fieldName) where TC : class
        {
            if (fieldName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(fieldName));
            if (child.IsNull()) throw new ArgumentNullException(nameof(child));

            OnAddEventHooks(child, fieldName);
        }

        /// <summary>
        /// Hook child object events.
        /// </summary>
        /// <param name="child">Child object.</param>
        /// <param name="fieldName">a name of a (private) field that manages child value</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnAddEventHooks<TC>(TC child, string fieldName) where TC : class
        {
            if (fieldName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(fieldName));
            if (child.IsNull()) throw new ArgumentNullException(nameof(child));

            bool hasAnyEvents = false;

            if (child is BindableBase bb)
            {
                bb.NotifyPropertyChangingEnabled = NotifyPropertyChangingEnabled;
                bb.BindingMode = BindingMode;
                bb.NotifyPropertyChangedEnabled = true;
            }

            if (child is INotifyPropertyChanged pc)
            {
                _propertyChangedDelegates[fieldName] = (o, e) =>
                    ChildHasChanged(fieldName, o, CreateChildChangedEventArgs(o, e));
                pc.PropertyChanged += _propertyChangedDelegates[fieldName];
                hasAnyEvents = true;
            }

            if (child is IBindingList bl)
            {
                _listChangedDelegates[fieldName] = (o, e) =>
                    ChildHasChanged(fieldName, o, CreateChildChangedEventArgs(o, null, e));
                bl.ListChanged += _listChangedDelegates[fieldName];
                hasAnyEvents = true;
            }
            else if (child is INotifyCollectionChanged ncc)
            {
                _collectionChangedDelegates[fieldName] = (o, e) =>
                    ChildHasChanged(fieldName, o, CreateChildChangedEventArgs(o, null, e));
                ncc.CollectionChanged += _collectionChangedDelegates[fieldName];
                hasAnyEvents = true;
            }

            if (child is INotifyChildChanged nchc)
            {
                _childChangedDelegates[fieldName] = (o, e) =>
                    ChildHasChanged(fieldName, o, e);
                nchc.ChildChanged += _childChangedDelegates[fieldName];
                hasAnyEvents = true;
            }

            if (hasAnyEvents) _childrenWithEvents.Add(fieldName);
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        /// <param name="child">Child object.</param>
        /// <param name="fieldName">a name of a (private) field that manages child value</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void RemoveEventHooks<TC>(TC child, string fieldName) where TC : class
        {
            if (fieldName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(fieldName));
            if (child.IsNull()) throw new ArgumentNullException(nameof(child));
            OnRemoveEventHooks(child, fieldName);
        }

        /// <summary>
        /// Unhook child object events.
        /// </summary>
        /// <param name="child">Child object.</param>
        /// <param name="fieldName">a name of a (private) field that manages child value</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnRemoveEventHooks<TC>(TC child, string fieldName) where TC : class
        {
            if (fieldName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(fieldName));
            if (child.IsNull()) throw new ArgumentNullException(nameof(child));

            if (child is INotifyPropertyChanged pc && _propertyChangedDelegates.ContainsKey(fieldName))
            {
                pc.PropertyChanged -= _propertyChangedDelegates[fieldName];
                _propertyChangedDelegates.Remove(fieldName);
            }

            if (child is IBindingList bl && _listChangedDelegates.ContainsKey(fieldName))
            {
                bl.ListChanged -= _listChangedDelegates[fieldName];
                _listChangedDelegates.Remove(fieldName);
            }
            else if (child is INotifyCollectionChanged ncc && _collectionChangedDelegates.ContainsKey(fieldName))
            {
                ncc.CollectionChanged -= _collectionChangedDelegates[fieldName];
                _collectionChangedDelegates.Remove(fieldName);
            }

            if (child is INotifyChildChanged nchc && _childChangedDelegates.ContainsKey(fieldName))
            {
                nchc.ChildChanged -= _childChangedDelegates[fieldName];
                _childChangedDelegates.Remove(fieldName);
            }

            if (_childrenWithEvents.Contains(fieldName)) _childrenWithEvents.Remove(fieldName);
        }


        /// <summary>
        /// Enables or disables child binding support for INotifyPropertyChanging
        /// when this setting is changed for this instance of domain entity.
        /// </summary>
        protected override void OnNotifyPropertyChangingEnabledChanged()
        {
            foreach (var bindableChildName in _bindableChildren)
            {
                var bindableChild = GetBindableChild(bindableChildName);
                if (!bindableChild.IsNull())
                    bindableChild.NotifyPropertyChangingEnabled = NotifyPropertyChangingEnabled;
            }
        }

        /// <summary>
        /// Enables or disables winforms binding support for INotifyPropertyChanged
        /// when this setting is changed for this instance of domain entity.
        /// (one event per property group)
        /// </summary>
        protected override void OnBindingModeChanged()
        {
            foreach (var bindableChildName in _bindableChildren)
            {
                var bindableChild = GetBindableChild(bindableChildName);
                if (!bindableChild.IsNull())
                    bindableChild.BindingMode = BindingMode;
            }
        }

        /// <summary>
        /// Enables or disables child binding support for INotifyPropertyChanged
        /// when this setting is changed for this instance of domain entity.
        /// </summary>
        /// <remarks>Cannot turn of INotifyPropertyChanged support for children
        /// that are hooked on this instance as that would likely impede internal functioning,
        /// e.g. loosing track of invoice items renders impossible to keep subtotals.</remarks>
        protected override void OnNotifyPropertyChangedEnabledChanged()
        {
            foreach (var bindableChildName in _bindableChildren)
            {
                if (_childrenWithEvents.Contains(bindableChildName))
                {
                    if (NotifyPropertyChangedEnabled)
                    {
                        var bindableChild = GetBindableChild(bindableChildName);
                        if (!bindableChild.IsNull())
                            bindableChild.NotifyPropertyChangedEnabled = true;
                    }
                }
                else
                {
                    var bindableChild = GetBindableChild(bindableChildName);
                    if (!bindableChild.IsNull())
                        bindableChild.NotifyPropertyChangedEnabled = NotifyPropertyChangedEnabled;
                }
            }
        }


        private void RestoreEventHooks()
        {
            foreach (var childWithEventName in _childrenWithEvents)
            {
                var childField = typeof(T).GetField(childWithEventName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (null == childField)
                {
                    var propField = typeof(T).GetProperty(childWithEventName);
                    if (null != propField)
                    {
                        var value = propField.GetValue(this);
                        if (!value.IsNull()) AddEventHooks(value, childWithEventName);
                    }
                }
                else
                {
                    var value = childField.GetValue(this);
                    if (!value.IsNull()) AddEventHooks(value, childWithEventName);
                }
            }
        }

        private IBindable GetBindableChild(string fieldName)
        {
            var childField = typeof(T).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (null == childField)
            {
                var childProp = typeof(T).GetProperty(fieldName);
                if (null != childProp)
                {
                    if (childProp.GetValue(this) is IBindable result && !result.IsNull())
                        return result;
                }
            }
            else if (childField.GetValue(this) is IBindable result && !result.IsNull())
                return result;

            return null;
        }

        #endregion

        #region Child Change Notification

        [NonSerialized]
        private EventHandler<ChildChangedEventArgs> _childChangedHandlers;

        /// <summary>
        /// Event raised when a child object has been changed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
          "CA1062:ValidateArgumentsOfPublicMethods")]
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
        /// <param name="e">ChildChangedEventArgs object.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnChildChanged(ChildChangedEventArgs e)
        {
            _childChangedHandlers?.Invoke(this, e); ;
        }


        private ChildChangedEventArgs CreateChildChangedEventArgs(object childObject,
            PropertyChangedEventArgs propertyArgs)
        {
            return new ChildChangedEventArgs(childObject, propertyArgs);
        }

        private ChildChangedEventArgs CreateChildChangedEventArgs(object childObject,
            PropertyChangedEventArgs propertyArgs, ListChangedEventArgs listArgs)
        {
            return new ChildChangedEventArgs(childObject, propertyArgs, listArgs);
        }

        private ChildChangedEventArgs CreateChildChangedEventArgs(object childObject,
            PropertyChangedEventArgs propertyArgs, NotifyCollectionChangedEventArgs listArgs)
        {
            return new ChildChangedEventArgs(childObject, propertyArgs, listArgs);
        }


        /// <summary>
        /// Bubbles up child change events. Override it to handle child changes.
        /// Call base at first, if you want to bubble events before your code executes and vice versa.
        /// </summary>
        /// <param name="fieldName">a name of the field (or property) that has changed</param>
        /// <param name="sender">a (child) object that raised the event</param>
        /// <param name="e">change event arguments</param>
        protected virtual void ChildHasChanged(string fieldName, object sender, ChildChangedEventArgs e)
        {
            if (e.ListChangedArgs.IsNull() || e.ListChangedArgs.ListChangedType != ListChangedType.ItemChanged)
                OnChildChanged(e);
        }

        #endregion

        #endregion

        #region Serialization Notification

        [System.Runtime.Serialization.OnDeserialized]
        private void OnDeserializedHandler(System.Runtime.Serialization.StreamingContext context)
        {
            RestoreEventHooks();
            OnDeserialized(context);
        }

        /// <summary>
        /// This method is called on a newly deserialized object after deserialization is complete.
        /// </summary>
        /// <param name="context">Serialization context object.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        { }

        #endregion

        #region Property Setters

        /// <summary>
        /// Property setter that shall be used to set child object properties of the following types:
        /// 1. BindableBase;
        /// 2. ITrackState;
        /// 3. Any other class type if its state should be tracked by hooking into its events and
        /// it implements INotifyPropertyChanging, NotifyPropertyChanged, IBindingList or INotifyCollectionChanged.
        /// </summary>
        /// <typeparam name="TValue">type of the child object</typeparam>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="fieldName">a name of the field that the property value is stored by</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="addEventHooks">whether to hook into the child object events</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are not the same instance (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. UnRegisterChildValue for old value;
        /// 5. Updates property value to the new one;
        /// 6. RegisterChildValue for new value;
        /// 7. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetChildPropertyValue<TValue>(string propertyName, string fieldName, 
            ref TValue oldValue, TValue newValue, bool addEventHooks = true, Action postProcessing = null) 
            where TValue : class
        {
            if (fieldName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(fieldName));
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!ReferenceEquals(oldValue, newValue))
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }

                OnPropertyChanging(propertyName);

                UnRegisterChildValue(oldValue, fieldName);

                oldValue = newValue;

                RegisterChildValue(oldValue, fieldName, addEventHooks);

                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set ILookup properties.
        /// </summary>
        /// <typeparam name="TValue">type of the ILookup implementation</typeparam>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same, i.e. reference the same domain entity (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetLookupPropertyValue<TValue, TRefValue>(string propertyName, ref TValue oldValue, 
            TValue newValue, Action postProcessing = null)
            where TValue : ILookup<TRefValue>
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if ((ILookup<TRefValue>)oldValue != (ILookup<TRefValue>)newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable decimal properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="ignoreCase">whether to ignore case when evaluating (but NOT setting) value</param>
        /// <param name="trimValue">whether to trim values when evaluating and setting value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref string oldValue, string newValue,
            bool ignoreCase = false, bool trimValue = true, Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!StringValuesEqual(oldValue, newValue, ignoreCase, trimValue))
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                
                if (trimValue) oldValue = (newValue ?? String.Empty).Trim();
                else oldValue = (newValue ?? String.Empty);
                
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set char properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref char oldValue, char newValue, 
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable char properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref char? oldValue, char? newValue, 
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set int properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref int oldValue, int newValue, 
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable int properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref int? oldValue, int? newValue, 
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set uint properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref uint oldValue, uint newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable uint properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref uint? oldValue, uint? newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set int16 properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref short oldValue, short newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable int16 properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref short? oldValue, short? newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set ushort properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref ushort oldValue, ushort newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable ushort properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref ushort? oldValue, ushort? newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set byte properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref byte oldValue, byte newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable byte properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref byte? oldValue, byte? newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set sbyte properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref sbyte oldValue, sbyte newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable sbyte properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref sbyte? oldValue, sbyte? newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set int64 properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref long oldValue, long newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable int64 properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref long? oldValue, long? newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false; 
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false; 
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set ulong properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref ulong oldValue, ulong newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false; 
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable ulong properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref ulong? oldValue, ulong? newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set decimal properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="digits">number of significant digits to evaluate and set (using AccountingRound extension)</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected virtual bool SetPropertyValue(string propertyName, ref decimal oldValue, decimal newValue, 
            int digits, Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue.AccountingRound(digits) != newValue.AccountingRound(digits))
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue.AccountingRound(digits);
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable decimal properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="digits">number of significant digits to evaluate and set (using AccountingRound extension)</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected virtual bool SetPropertyValue(string propertyName, ref decimal? oldValue, decimal? newValue, 
            int digits, Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;

            if (!oldValue.HasValue || !newValue.HasValue || 
                oldValue.Value.AccountingRound(digits) != newValue.Value.AccountingRound(digits))
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                if (newValue.HasValue) oldValue = newValue.Value.AccountingRound(digits);
                else oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set double properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="digits">number of significant digits to evaluate and set (using AccountingRound extension)</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected virtual bool SetPropertyValue(string propertyName, ref double oldValue, double newValue, 
            int digits, Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.EqualsTo(newValue, digits))
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue.AccountingRound(digits);
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable double properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="digits">number of significant digits to evaluate and set (using AccountingRound extension)</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected virtual bool SetPropertyValue(string propertyName, ref double? oldValue, double? newValue, 
            int digits, Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;

            if (!oldValue.HasValue || !newValue.HasValue || !oldValue.Value.EqualsTo(newValue.Value, digits))
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                if (newValue.HasValue) oldValue = newValue.Value.AccountingRound(digits);
                else oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set DateTime properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="ignoreTime">whether to trim time part when evaluating and setting value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref DateTime oldValue, DateTime newValue,
            bool ignoreTime = true, Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            bool sameValues;
            if (ignoreTime) sameValues = (oldValue.Date == newValue.Date);
            else sameValues = (oldValue == newValue);

            if (!sameValues)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                if (ignoreTime) oldValue = newValue.Date;
                else oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable DateTime properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="ignoreTime">whether to trim time part when evaluating and setting value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref DateTime? oldValue, DateTime? newValue,
            bool ignoreTime = true, Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;

            bool sameValues;
            if (!oldValue.HasValue || !newValue.HasValue) sameValues = false;
            else if (ignoreTime) sameValues = (oldValue.Value.Date == newValue.Value.Date);
            else sameValues = (oldValue.Value == newValue.Value);

            if (!sameValues)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                if (ignoreTime && newValue.HasValue) oldValue = newValue.Value.Date;
                else oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set bool properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref bool oldValue, bool newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set nullable bool properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue(string propertyName, ref bool? oldValue, bool? newValue,
            Action postProcessing = null)
        {
            if (propertyName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(propertyName));

            if (!oldValue.HasValue && !newValue.HasValue) return false;
            if (oldValue != newValue)
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Property setter that shall be used to set Enum properties.
        /// </summary>
        /// <param name="propertyName">a name of the property</param>
        /// <param name="oldValue">current field value</param>
        /// <param name="newValue">new field value</param>
        /// <param name="postProcessing">an action to be executed after the property value is set;
        /// if you specify the post processing action, it shall invoke PropertyHasChanged
        /// (or PropertiesHaveChanged)</param>
        /// <remarks>Does the following:
        /// 1. Checks if the new and old values are the same (if so, returns false);
        /// 2. Checks if the property is not locked due to the entity state (if so, raises binding event and returns false);
        /// 3. Raises OnPropertyChanging (i.e. binding event);
        /// 4. Updates property value to the new one;
        /// 5. Invokes post processing action or, if it's null, raises PropertyHasChanged
        /// (i.e. checks rules and raises binding event).</remarks>
        /// <returns>true if the property has been changed, otherwise false</returns>
        protected bool SetPropertyValue<TEnum>(string propertyName, ref TEnum oldValue, TEnum newValue,
            Action postProcessing = null) 
            where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum) throw new InvalidOperationException(
                $"Method ApplyPropValue<T> only supports ENUM's while the type passed is {typeof(TEnum).FullName}.");

            if (!EqualityComparer<TEnum>.Default.Equals(newValue, oldValue))
            {
                if (!CanWriteProperty(propertyName))
                {
                    // notify UI to update value back to the old one
                    OnPropertiesChanged(propertyName);
                    return false;
                }
                OnPropertyChanging(propertyName);
                oldValue = newValue;
                if (null != postProcessing) postProcessing();
                else PropertyHasChanged(propertyName);
                return true;
            }
            return false;
        }


        private bool StringValuesEqual(string oldValue, string newValue, bool ignoreCase = false, bool trimValue = true)
        {
            bool result;
            if (trimValue)
            {
                if (ignoreCase)
                {
                    result = ((oldValue ?? string.Empty).Trim()
                        .Equals((newValue ?? string.Empty).Trim(), StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    result = ((oldValue ?? string.Empty).Trim()
                        .Equals((newValue ?? string.Empty).Trim()));
                }
            }
            else
            {
                if (ignoreCase)
                {
                    result = ((oldValue ?? string.Empty)
                        .Equals((newValue ?? string.Empty), StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    result = ((oldValue ?? string.Empty)
                        .Equals((newValue ?? string.Empty)));
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// apply with using pattern to temporally disable validation
        /// </summary>
        private sealed class SuspendValidationInt<TC> : IDisposable where TC : DomainObject<T>
        {
            private readonly TC _entity;
            private bool disposedValue;

            /// <summary>
            /// apply with using pattern to temporally disable validation
            /// </summary>
            /// <param name="entity">a domain object to temporally disable validation for</param>
            public SuspendValidationInt(TC entity)
            {
                _entity = entity ?? throw new ArgumentNullException(nameof(entity));
                _entity._suspendValidation = true;
            }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _entity._suspendValidation = false;
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

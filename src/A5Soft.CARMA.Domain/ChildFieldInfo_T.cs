using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace A5Soft.CARMA.Domain
{
    internal class ChildFieldInfo<TClass, TField> : ChildFieldInfo<TClass>
        where TClass : DomainObject<TClass>
        where TField : class
    {
        [NonSerialized]
        private PropertyChangedEventHandler _propertyChangedDelegate = null;
        [NonSerialized]
        private ListChangedEventHandler _listChangedDelegate = null;
        [NonSerialized]
        private NotifyCollectionChangedEventHandler _collectionChangedDelegate;
        [NonSerialized]
        private EventHandler<ChildChangedEventArgs> _childChangedDelegate;


        private Func<TClass, TField> Getter { get; }

        public override Type FieldType => typeof(TField);

        
        public ChildFieldInfo(Expression<Func<TClass, TField>> fieldExpression, string propertyName)
            : base(propertyName)
        {
            var fieldInfo = GetFieldInfo(fieldExpression);
            FieldName = fieldInfo.Name;
            Getter = fieldExpression.Compile();
            IsBindable = typeof(IBindable).IsAssignableFrom(typeof(TField));
            IsStateful = typeof(ITrackState).IsAssignableFrom(typeof(TField));
            IsINotifyPropertyChanged = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(TField));
            IsIBindingList = typeof(IBindingList).IsAssignableFrom(typeof(TField));
            IsINotifyCollectionChanged = typeof(INotifyCollectionChanged).IsAssignableFrom(typeof(TField));
            IsINotifyChildChanged = typeof(INotifyChildChanged).IsAssignableFrom(typeof(TField));

        }


        public override void AddEventsFor(TClass parent)
        {
            WithEvents = false;

            var child = Getter(parent);
            if (ReferenceEquals(child, null)) return;

            if (child is BindableBase bb)
            {
                bb.NotifyPropertyChangingEnabled = parent.NotifyPropertyChangingEnabled;
                bb.BindingMode = parent.BindingMode;
                bb.NotifyPropertyChangedEnabled = true;
            }

            if (child is INotifyPropertyChanged pc)
            {
                _propertyChangedDelegate = (o, e) =>
                    parent.ChildHasChanged(FieldName, o, CreateChildChangedEventArgs(o, e));
                pc.PropertyChanged += _propertyChangedDelegate;
                WithEvents = true;
            }

            if (child is IBindingList bl)
            {
                _listChangedDelegate = (o, e) =>
                    parent.ChildHasChanged(FieldName, o, CreateChildChangedEventArgs(o, null, e));
                bl.ListChanged += _listChangedDelegate;
                WithEvents = true;
            }
            else if (child is INotifyCollectionChanged ncc)
            {
                _collectionChangedDelegate = (o, e) =>
                    parent.ChildHasChanged(FieldName, o, CreateChildChangedEventArgs(o, null, e));
                ncc.CollectionChanged += _collectionChangedDelegate;
                WithEvents = true;
            }

            if (child is INotifyChildChanged nchc)
            {
                _childChangedDelegate = (o, e) =>
                    parent.ChildHasChanged(FieldName, o, e);
                nchc.ChildChanged += _childChangedDelegate;
                WithEvents = true;
            }
        }

        public override void RemoveEventsFor(TClass parent)
        {
            WithEvents = false;

            var child = Getter(parent);
            if (ReferenceEquals(child, null)) return;

            if (child is INotifyPropertyChanged pc)
            {
                pc.PropertyChanged -= _propertyChangedDelegate;
                _propertyChangedDelegate = null;
            }

            if (child is IBindingList bl)
            {
                bl.ListChanged -= _listChangedDelegate;
                _listChangedDelegate = null;
            }
            else if (child is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= _collectionChangedDelegate;
                _collectionChangedDelegate = null;
            }

            if (child is INotifyChildChanged nchc)
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

        private static FieldInfo GetFieldInfo(Expression<Func<TClass, TField>> fieldExpression)
        {
            if (null == fieldExpression) throw new ArgumentNullException(nameof(fieldExpression));

            if (fieldExpression.Body is MemberExpression memberExpression &&
                memberExpression.Member is FieldInfo fieldInfo) return fieldInfo;

            throw new ArgumentException("Expression must be a field access expression",
                nameof(fieldExpression));
        }
    }
}

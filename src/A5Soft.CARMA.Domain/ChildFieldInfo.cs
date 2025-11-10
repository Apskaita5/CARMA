using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain
{
    internal abstract class ChildFieldInfo<TClass>
        where TClass : DomainObject<TClass>
    {
        protected ChildFieldInfo(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            PropertyName = propertyName;
        }


        public string PropertyName { get; }

        public abstract Type FieldType { get; }

        public string FieldName { get; protected set; }

        public bool IsBindable { get; protected set; }

        public bool IsStateful { get; protected set; }

        public bool IsINotifyPropertyChanged { get; protected set; }

        public bool IsIBindingList { get; protected set; }

        public bool IsINotifyCollectionChanged { get; protected set; }

        public bool IsINotifyChildChanged { get; protected set; }

        public bool WithEvents { get; protected set; }


        public abstract void AddEventsFor(TClass parent);

        public abstract void RemoveEventsFor(TClass parent);
    }
}

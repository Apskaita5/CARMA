using A5Soft.CARMA.Domain.Rules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Handles the management of child fields for a given TClass.
    /// </summary>
     internal class ChildrenManager<TClass>
        where TClass : DomainObject<TClass>
    {
        private static readonly ConcurrentDictionary<Type, object> _cache =
            new ConcurrentDictionary<Type, object>();
        private TClass _parent;


        /// <summary>
        /// Initializes an instance of the ChildrenManager class.
        /// This constructor uses a specified parent and retrieves child fields from this parent to manage them.
        /// </summary>
        /// <param name="parent"> The parent object of the type TClass.</param>
        public ChildrenManager(TClass parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            var info = (ChildrenInfo<TClass>)_cache.GetOrAdd(typeof(TClass),
                t => new ChildrenInfo<TClass>());

            ChildFields = info.ChildFields
                .Select(f => new ChildFieldManager<TClass>(f))
                .ToList();
        }

        /// <summary>
        /// Gets a read-only collection of child field managers.
        /// </summary>
        public IReadOnlyCollection<ChildFieldManager<TClass>> ChildFields { get; }


        /// <summary>
        /// Unregisters the child value for a given field name.
        /// </summary>
        /// <param name="fieldName">The field name to unregister.</param>
        /// <exception cref="ArgumentNullException">Thrown when fieldName is null or white space.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no such child field exists for the given fieldName.</exception>
        public void UnregisterChildValueFor(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            var field = ChildFields
                .FirstOrDefault(f => f.FieldName.Equals(fieldName));
            if (null == field) throw new InvalidOperationException(
                $"No such child field {fieldName} for type {typeof(TClass).FullName}.");

            field.RemoveEventsFor(_parent);
        }

        /// <summary>
        /// This method is used to register a child value for a particular field in the parent class.
        /// </summary>
        /// <param name="fieldName">The name of the field for which the child value needs to be registered.</param>
        /// <exception cref="ArgumentNullException">If the provided fieldName is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">If no such field exists in the parent class.</exception>
        public void RegisterChildValueFor(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            var field = ChildFields
                .FirstOrDefault(f => f.FieldName.Equals(fieldName));
            if (null == field) throw new InvalidOperationException(
                $"No such child field {fieldName} for type {typeof(TClass).FullName}.");

            field.AddEventsFor(_parent);
        }

        /// <summary>
        /// Called when binding options change. Updates binding options for each bindable child field in relation to its parent.
        /// </summary>
        public void OnBindingOptionsChanged()
        {
            foreach (var item in ChildFields.Where(c => c.IsBindable))
            {
                item.UpdateBindingOptions(_parent);
            }
        }

        /// <summary>
        /// Restores all the event handlers for each of the child fields associated with the Parent object.
        /// </summary>
        public void RestoreEventHooks()
        {
            foreach (var item in ChildFields)
            {
                item.AddEventsFor(_parent);
            }
        }

        /// <summary>
        /// Sets the validation engine provider for the current object and its child fields.
        /// Throws an exception if the provided validation engine is null.
        /// </summary>
        /// <param name="validationEngineProvider">The IValidationEngineProvider instance that is to be set as the validation engine.</param>
        public void SetValidationEngine(IValidationEngineProvider validationEngineProvider)
        {
            if (null == validationEngineProvider) throw new ArgumentNullException(nameof(validationEngineProvider));

            foreach (var item in ChildFields)
            {
                item.GetChildAsStateful(_parent)?.SetValidationEngine(validationEngineProvider);
            }
        }

        /// <summary>
        /// Checks if the collection ChildFields contains any new data.
        /// This is determined by looping through each item and checking if it has new data.
        /// </summary>
        /// <returns>
        /// Returns true if any item in ChildFields contains new data. Else, returns false.
        /// </returns>
        public bool ContainsNewData()
        {
            foreach (var item in ChildFields)
            {
                var result = item.GetChildAsStateful(_parent)?.ContainsNewData;
                if (result.HasValue && result.Value) return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether any child field of a parent item has been modified since the last save operation.
        /// </summary>
        /// <returns>
        /// True if at least one child field under the parent item has been modified; otherwise false.
        /// </returns>
        public bool IsDirty()
        {
            foreach (var item in ChildFields)
            {
                var result = item.GetChildAsStateful(_parent)?.IsDirty;
                if (result.HasValue && result.Value) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether any of the child fields has warnings.
        /// </summary>
        /// <returns>
        /// Returns true if any child field has warnings. Otherwise, it returns false.
        /// </returns>
        public bool HasWarnings()
        {
            foreach (var item in ChildFields)
            {
                var result = item.GetChildAsStateful(_parent)?.HasWarnings;
                if (result.HasValue && result.Value) return true;
            }
            return false;
        }

        /// <summary>
        /// Validates the child fields of the parent object. Loops through each child field, checks if it's valid.
        /// If any child field is not valid, the method returns false.
        /// </summary>
        /// <returns>
        /// Returns a boolean value indicating whether all child fields are valid. If all fields are valid, it returns true. Otherwise, false.
        /// </returns>
        public bool IsValid()
        {
            foreach (var item in ChildFields)
            {
                var result = item.GetChildAsStateful(_parent)?.IsValid;
                if (result.HasValue && !result.Value) return false;
            }
            return true;
        }

        /// <summary>
        /// This method is used to check the rules for each child item in the list of ChildFields.
        /// It iterates over each child item, retrieves its state and checks the defined rules for that state.
        /// </summary>
        public void CheckRules()
        {
            foreach (var item in ChildFields)
            {
                item.GetChildAsStateful(_parent)?.CheckRules();
            }
        }

        /// <summary>
        /// Adds the broken rules of children to the specified BrokenRulesTreeNode.
        /// </summary>
        /// <param name="treeNode">The BrokenRulesTreeNode to which the broken rules are to be added.
        /// If it's null, an ArgumentNullException will be thrown.</param>
        public void AddChildrenBrokenRules(BrokenRulesTreeNode treeNode)
        {
            if (null == treeNode) throw new ArgumentNullException(nameof(treeNode));

            foreach (var item in ChildFields)
            {
                var stateful = item.GetChildAsStateful(_parent);
                if (null != stateful && !stateful.IsValid)
                    treeNode.AddBrokenRulesForChild(stateful.GetBrokenRulesTree(false));
            }
        }
    }
}
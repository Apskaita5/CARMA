using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Defines the common properties required by objects that track their own state.
    /// </summary>
    public interface ITrackState : IDomainObject
    {

        /// <summary>
        /// Returns true if the object and its child objects are currently valid, 
        /// false if the object or any of its child objects have broken rules or are otherwise invalid.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default this property relies on the underling ValidationRules
        /// object to track whether any business rules are currently broken for this object.
        /// </para><para>
        /// You can override this property to provide more sophisticated
        /// implementations of the behavior. 
        /// </para>
        /// </remarks>
        /// <returns>A value indicating if the object is currently valid.</returns>
        bool IsValid { get; }
        
        /// <summary>
        /// Returns true if the object is currently valid, false if the
        /// object has broken rules or is otherwise invalid.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default this property relies on the underling ValidationRules
        /// object to track whether any business rules are currently broken for this object.
        /// </para><para>
        /// You can override this property to provide more sophisticated
        /// implementations of the behavior.
        /// </para>
        /// </remarks>
        /// <returns>A value indicating if the object is currently valid.</returns>
        bool IsSelfValid { get; }

        /// <summary>
        /// Returns true if the object and its child objects have warning level broken rules
        /// (that do not render entity invalid, yet it's sensible to ask user for confirmation), 
        /// false if the object or any of its child objects have no warning level broken rules.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default this property relies on the underling ValidationRules
        /// object to track whether any business rules are currently broken for this object.
        /// </para><para>
        /// You can override this property to provide more sophisticated
        /// implementations of the behavior. 
        /// </para>
        /// </remarks>
        /// <returns>A value indicating if the object or its child objects have warning level broken rules</returns>
        bool HasWarnings { get; }

        /// <summary>
        /// Returns true if the object has warning level broken rules (that do not render entity invalid,
        /// yet it's sensible to ask user for confirmation), false if the object has no warning level broken rules.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default this property relies on the underling ValidationRules
        /// object to track whether any business rules are currently broken for this object.
        /// </para><para>
        /// You can override this property to provide more sophisticated
        /// implementations of the behavior.
        /// </para>
        /// </remarks>
        /// <returns>A value indicating if the object has warning level broken rules</returns>
        bool HasSelfWarnings { get; }
         
        /// <summary>
        /// Returns true if this object's data, or any of its fields or child objects data, 
        /// has been changed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When an object's data is changed, application note of that change
        /// and considers the object to be 'dirty' or changed. This value is used to
        /// optimize data updates, since an unchanged object does not need to be
        /// updated into the database. All new objects are considered dirty. All objects
        /// marked for deletion are considered dirty.
        /// </para><para>
        /// Once an object's data has been saved to the database (inserted or updated)
        /// the dirty flag is cleared and the object is considered unchanged. Objects
        /// newly loaded from the database are also considered unchanged.
        /// </para>
        /// </remarks>
        /// <returns>A value indicating if this object's data has been changed.</returns>
        bool IsDirty { get; }
        
        /// <summary>
        /// Returns true if this object's data has been changed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When an object's data is changed, application makes note of that change
        /// and considers the object to be 'dirty' or changed. This value is used to
        /// optimize data updates, since an unchanged object does not need to be
        /// updated into the database. All new objects are considered dirty. All objects
        /// marked for deletion are considered dirty.
        /// </para><para>
        /// Once an object's data has been saved to the database (inserted or updated)
        /// the dirty flag is cleared and the object is considered unchanged. Objects
        /// newly loaded from the database are also considered unchanged.
        /// </para>
        /// </remarks>
        /// <returns>A value indicating if this object's data has been changed.</returns>
        bool IsSelfDirty { get; }

        /// <summary>
        /// Returns true if this object's data, or any of its child objects data, 
        /// has been changed.
        /// </summary>
        /// <remarks>
        /// In contrast to IsDirty tracks data changes for both new and old entities,
        /// i.e. new entity is always considered dirty, yet no data has been changed.
        /// </remarks>
        /// <returns>A value indicating if this object's or any of its child objects data has been changed.</returns>
        bool ContainsNewData { get; }

        /// <summary>
        /// Returns true if this object's data has been changed.
        /// </summary>
        /// <remarks>
        /// In contrast to IsDirty tracks data changes for both new and old entities,
        /// i.e. new entity is always considered dirty, yet no data has been changed.
        /// </remarks>
        /// <returns>A value indicating if this object's data has been changed.</returns>
        bool SelfContainsNewData { get; }

        /// <summary>
        /// Returns true if this object is marked for deletion.
        /// </summary>
        /// <remarks>
        /// Application supports both immediate and deferred deletion of objects. This
        /// property is part of the support for deferred deletion, where an object
        /// can be marked for deletion, but isn't actually deleted until the object
        /// is saved to the database. This property indicates whether or not the
        /// current object has been marked for deletion. If it is true
        /// , the object will be deleted when it is saved to the database,
        /// otherwise it will be inserted or updated by the save operation.
        /// </remarks>
        /// <returns>A value indicating if this object is marked for deletion.</returns>
        bool IsDeleted { get; }

        /// <summary>
        /// Returns true if this object is both dirty and valid.
        /// </summary>
        /// <remarks>
        /// An object is considered dirty (changed) if <see cref="IsDirty" /> returns true. It is
        /// considered valid if IsValid returns true. The IsSavable property is
        /// a combination of these two properties. 
        /// </remarks>
        /// <returns>A value indicating if this object is both dirty and valid.</returns>
        bool IsSavable { get; }
        
        /// <summary>
        /// Returns true if this is a child object, false if it is a root object.
        /// </summary>
        /// <remarks>
        /// An object should be marked as a child object unless it is the
        /// top most object (root object) in a domain object graph.
        /// </remarks>
        /// <returns>A value indicating if this a child object.</returns>
        bool IsChild { get; }

        /// <summary>
        /// Gets a collection of properties that are not allowed to be written due to the current entity state.
        /// </summary>
        /// <remarks>Override this property to implement property locking logic.
        /// This property is meant for UI only to conditionally disable UI controls for locked properties.</remarks>
        string[] LockedProperties { get; }


        /// <summary>
        /// Gets a value indicating whether the current entity state allows changing the property specified.
        /// </summary>
        /// <param name="propertyName">a name of the property to check for the write access</param>
        /// <returns>a value indicating whether the current entity state allows changing the property specified</returns>
        bool CanWriteProperty(string propertyName);

        /// <summary>
        /// Sets a validation engine to use by the entity.
        /// Shall be invoked after deserialization if a custom validation engine is used.
        /// </summary>
        /// <param name="validationEngineProvider">a validation engine provider to use</param>
        void SetValidationEngine(IValidationEngineProvider validationEngineProvider);

        /// <summary>
        /// Forces check of all the business rules for the entity.
        /// </summary>
        /// <param name="checkRulesForChildren">whether to invoke CheckRules method on children</param>
        void CheckRules(bool checkRulesForChildren = true);

        /// <summary>
        /// Gets a hierarchical collection of currently broken rules for both an entity and all its child entities.
        /// </summary>
        /// <param name="useInstanceDescription">whether to use instance description instead of
        /// an entity class description (for internal use only)</param>
        BrokenRulesTreeNode GetBrokenRulesTree(bool useInstanceDescription = false);

    }
}

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A base class for all domain entities that have an identity (i.e. more than one entity
    /// of the type exists in the domain) yet have no auditing functionality.
    /// </summary>
    /// <typeparam name="T">a type of the domain entity implementation</typeparam> 
    [Serializable]
    public abstract class DomainEntity<T> : DomainObject<T>, IDomainEntity
        where T : DomainEntity<T>
    {

        #region Constructors

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        protected DomainEntity(IValidationEngineProvider validationEngineProvider) 
            : base(validationEngineProvider)
        {
            _id = CreateNewIdentity();
        }

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        protected DomainEntity(IDomainEntityIdentity identity, IValidationEngineProvider validationEngineProvider, 
            bool allowNewIdentity = false) : base(validationEngineProvider)
        {
            if (!allowNewIdentity && (identity?.IsNew ?? true)) throw new InvalidOperationException(
                "Cannot set new (null) identity for an existing entity.");
            if (identity.IsNull()) identity = CreateNewIdentity();
            EnsureValidIdentity(identity);
            _id = identity;
        }

        #endregion

        #region Identity

        private IDomainEntityIdentity _id;

        public IDomainEntityIdentity Id
            => _id;

        /// <summary>
        /// Implement this method to create a new identity for the domain entity.
        /// </summary>
        /// <returns>a new identity for the domain entity</returns>
        protected abstract IDomainEntityIdentity CreateNewIdentity();

        #endregion

        /// <summary>
        /// Returns true if this is a new object, false if it is a pre-existing object.
        /// </summary>
        /// <remarks>
        /// An object is considered to be new if its primary identifying (key) value 
        /// doesn't correspond to data in the database. In other words, if the data values
        /// in this particular object have not yet been saved to the database
        /// the object is considered to be new. Likewise, if the object's data has been deleted
        /// from the database then the object is considered to be new.
        /// </remarks>
        /// <returns>A value indicating if this object is new.</returns>
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public bool IsNew
            => _id.IsNew;

        /// <summary>
        /// Marks the object as being a new object. This also marks the object
        /// as being dirty and ensures that it is not marked for deletion.
        /// </summary>
        /// <remarks>
        /// If you override this method, make sure to call the base
        /// implementation after executing your new code.
        /// </remarks>
        protected override void MarkNew()
        {
            _id = CreateNewIdentity();
            base.MarkNew();
        }

        /// <summary>
        /// Marks the object as being an old (not new) object. This also
        /// marks the object as being unchanged (not dirty).
        /// </summary>
        /// <remarks>
        /// <para>
        /// You should call this method in the implementation of
        /// DataPortal_Fetch to indicate that an existing object has been
        /// successfully retrieved from the database.
        /// </para><para>
        /// You should call this method in the implementation of 
        /// DataPortal_Update to indicate that a new object has been successfully
        /// inserted into the database.
        /// </para><para>
        /// If you override this method, make sure to call the base
        /// implementation after executing your new code.
        /// </para>
        /// </remarks>
        protected virtual void MarkOld(IDomainEntityIdentity identity)
        {
            EnsureValidIdentity(identity);
            _id = identity;
            MarkClean();
        }


        private void EnsureValidIdentity(IDomainEntityIdentity identity)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            if (identity.DomainEntityType != typeof(T)) throw new ArgumentException(
                $"Invalid identity, expected entity type - {typeof(T).FullName}, actual type - {identity.DomainEntityType.FullName}.", 
                nameof(identity));
            _id = identity;
        }

    }
}

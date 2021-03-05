using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A base class for all domain entities that have an identity (i.e. more than one entity
    /// of the type exists in the domain and they are distinguished by identity value)
    /// and auditing functionality (IAuditableEntity) implemented.
    /// </summary>
    /// <typeparam name="T">a type of the domain entity implementation</typeparam> 
    [Serializable]
    public abstract class AuditableDomainEntity<T> : DomainEntity<T>, IAuditableEntity
        where T : AuditableDomainEntity<T>
    {
        #region Private Fields

        private DateTime? _insertedAt = null;
        private string _insertedBy = string.Empty;
        private DateTime? _updatedAt = null;
        private string _updatedBy = string.Empty;
        private readonly string _occHash = string.Empty;

        #endregion
         
        #region Constructors

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        protected AuditableDomainEntity(IValidationEngineProvider validationEngineProvider) 
            : base(validationEngineProvider)
        { }

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        protected AuditableDomainEntity(IDomainEntityIdentity identity, 
            IValidationEngineProvider validationEngineProvider, DateTime insertedAt, string insertedBy,
            DateTime updatedAt, string updatedBy) 
            : base(identity, validationEngineProvider)
        {
            _insertedBy = insertedBy ?? string.Empty;
            _insertedAt = insertedAt;
            _updatedBy = updatedBy ?? string.Empty;
            _updatedAt = updatedAt;
            _occHash = (typeof(T).FullName + updatedAt.Ticks.ToString()).ComputeSha256Hash();
        }

        #endregion

        #region Properties

        /// <inheritdoc cref="IAuditableEntity.InsertedAt"/> 
        [Browsable(true)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public DateTime? InsertedAt
            => _insertedAt;

        /// <inheritdoc cref="IAuditableEntity.InsertedBy"/>   
        [Browsable(true)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public string InsertedBy
            => _insertedBy;

        /// <inheritdoc cref="IAuditableEntity.UpdatedAt"/>  
        [Browsable(true)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public DateTime? UpdatedAt
            => _updatedAt;

        /// <inheritdoc cref="IAuditableEntity.UpdatedBy"/>  
        [Browsable(true)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public string UpdatedBy
            => _updatedBy;

        /// <inheritdoc cref="IAuditableEntity.OccHash"/> 
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        public string OccHash
            => _occHash;

        #endregion

        /// <inheritdoc cref="IAuditableEntity.HideAuditTrace"/>  
        public void HideAuditTrace()
        {
            _updatedAt = _insertedAt = null;
            _updatedBy = _insertedBy = string.Empty;
        }

    }
}

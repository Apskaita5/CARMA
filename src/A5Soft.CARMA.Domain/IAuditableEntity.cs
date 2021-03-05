﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Common interface for the entities that are auditable,
    /// i.e. include data on when it was created and last updated
    /// as well as identifiers of the users who created and last updated the entity.
    /// </summary>
    public interface IAuditableEntity
    {

        /// <summary>
        /// Gets an UTC date and time when the entity was created.
        /// Returns null for a new entity not yet saved to the database.
        /// </summary>
        [Browsable(true)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        DateTime? InsertedAt { get; }

        /// <summary>
        /// Gets an identifier (e.g. email) of the user who created the entity.
        /// Returns string.Empty for a new entity not yet saved to the database.
        /// </summary> 
        [Browsable(true)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        string InsertedBy { get; }

        /// <summary>
        /// Gets an UTC date and time when the entity was last updated.
        /// Returns null for a new entity not yet saved to the database.
        /// Returns the same value as InsertedAt, if the entity has never been updated.
        /// </summary>      
        [Browsable(true)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        DateTime? UpdatedAt { get; }

        /// <summary>
        /// Gets an identifier (e.g. email) of the user who updated the entity last.
        /// Returns string.Empty for a new entity not yet saved to the database.
        /// Returns the same value as InsertedBy, if the entity has never been updated.
        /// </summary>   
        [Browsable(true)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        string UpdatedBy { get; }

        /// <summary>
        /// Gets a hash of UpdatedAt for optimistic concurrency control.
        /// </summary>
        /// <remarks>UpdatedAt value shall not be used as a user might not be authorized to access audit trail data.</remarks>
        [Browsable(false)]
        [Display(AutoGenerateField = false)]
        [ScaffoldColumn(false)]
        [IgnorePropertyMetadata]
        string OccHash { get; }



        /// <summary>
        /// Hides audit trail data, i.e. sets all audit trail properties to default values.
        /// Used to hide audit trail data from a user who is not authorized to access audit trail data. 
        /// </summary>
        void HideAuditTrace();

    }
}

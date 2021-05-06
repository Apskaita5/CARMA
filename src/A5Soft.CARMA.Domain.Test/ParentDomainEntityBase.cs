using System;
using System.Collections.Generic;
using System.Text;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain.Test
{
    public class ParentDomainEntityBase : DomainEntityBase<ParentDomainEntityBase>
    {
        #region Private Fields

        private string _groupName = string.Empty;
        private int _maxUsers = 0;
        private int _maxTenants = 0;
        private ChildDomainEntityBase _childEntity;

        #endregion

        /// <inheritdoc />
        public ParentDomainEntityBase(IValidationEngineProvider validationEngineProvider) : base(validationEngineProvider)
        {
            SetChildField(nameof(_childEntity), ref _childEntity, new ChildDomainEntityBase(validationEngineProvider));
        }

        #region Properties

        public string GroupName
        {
            get => _groupName;
            set => SetPropertyValue(nameof(GroupName), ref _groupName, value);
        }


        public int MaxUsers
        {
            get => _maxUsers;
            set => SetPropertyValue(nameof(MaxUsers), ref _maxUsers, value);
        }


        public int MaxTenants
        {
            get => _maxTenants;
            set => SetPropertyValue(nameof(MaxTenants), ref _maxTenants, value);
        }

        public ChildDomainEntityBase ChildEntity
        {
            get => _childEntity;
        }

        #endregion

    }
}

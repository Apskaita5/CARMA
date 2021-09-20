using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain.Test
{
    public class SimpleDomainEntityBase : DomainObject<SimpleDomainEntityBase>
    {
        #region Private Fields

        private string _groupName = string.Empty;
        private int _maxUsers = 0;
        private int _maxTenants = 0;

        #endregion


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

        #endregion

        /// <inheritdoc />
        public SimpleDomainEntityBase(IValidationEngineProvider validationEngineProvider) : base(validationEngineProvider)
        {
        }



        

    }
}

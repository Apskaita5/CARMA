using System;
using System.Collections.Generic;
using System.Text;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain.Test
{
    public class ChildDomainEntityBase : DomainEntityBase<ChildDomainEntityBase>
    {
        #region Private Fields

        private string _stringProperty = string.Empty;
        private int _intProperty = 0;

        #endregion

        /// <inheritdoc />
        public ChildDomainEntityBase(IValidationEngineProvider validationEngineProvider) : base(validationEngineProvider)
        {
        }

        public string StringProperty
        {
            get => _stringProperty;
            set => SetPropertyValue(nameof(StringProperty), ref _stringProperty, value);
        }


        public int IntProperty
        {
            get => _intProperty;
            set => SetPropertyValue(nameof(IntProperty), ref _intProperty, value);
        }

    }
}

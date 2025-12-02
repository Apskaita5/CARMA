using A5Soft.CARMA.Domain.Rules;
using System;
using System.Reflection;

namespace A5Soft.CARMA.Domain.Test.TestEntities
{
    /// <summary>
    /// Test entity for DomainObject tests
    /// </summary>
    public class TestDomainEntity : DomainObject<TestDomainEntity>
    {
        private string _name = string.Empty;
        private int _age;
        private decimal _salary;
        private DateTime _birthDate;
        private bool _isActive;
        [ChildField(nameof(Child))]
        private TestChildEntity _child;

        public TestDomainEntity(IValidationEngineProvider validationProvider)
            : base(validationProvider)
        {
        }

        public TestDomainEntity(TestDomainEntity source)
            : base(source)
        {
            _name = source.Name;
            _age = source.Age;
            _birthDate = source.BirthDate;
            _child = source.Child;
            _isActive = source.IsActive;
            _salary = source.Salary;
        }

        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        public int Age
        {
            get => _age;
            set => SetPropertyValue(nameof(Age), ref _age, value);
        }

        public decimal Salary
        {
            get => _salary;
            set => SetPropertyValue(nameof(Salary), ref _salary, value, 2);
        }

        public DateTime BirthDate
        {
            get => _birthDate;
            set => SetPropertyValue(nameof(BirthDate), ref _birthDate, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
        }

        public TestChildEntity Child
        {
            get => _child;
            set => SetChildPropertyValue(nameof(Child), nameof(_child), ref _child, value);
        }

        // Expose protected methods for testing
        public void TestMarkDirty() => MarkDirty();
        public void TestMarkClean() => MarkClean();
        public void TestMarkNew() => MarkNew();
        public void TestMarkDeleted() => MarkDeleted();
        public void TestPropertyHasChanged(string propertyName) => PropertyHasChanged(propertyName);
        public void TestCheckPropertyRules(params string[] propertyNames) => CheckPropertyRules(propertyNames);
        public void TestCheckObjectRules() => CheckObjectRules();
    }
}

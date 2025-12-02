using A5Soft.CARMA.Domain.Rules;
using System.Reflection;

namespace A5Soft.CARMA.Domain.Test.TestEntities
{
    /// <summary>
    /// Test child entity
    /// </summary>
    public class TestChildEntity : DomainObject<TestChildEntity>
    {
        private string _description = string.Empty;

        public TestChildEntity(IValidationEngineProvider validationProvider)
            : base(validationProvider)
        {
        }

        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }

        public void DeleteChild()
        {
            var deleteChildMethod = this.GetType().GetMethod("DeleteChild", BindingFlags.Instance | BindingFlags.NonPublic);
            deleteChildMethod.Invoke(this, null);
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

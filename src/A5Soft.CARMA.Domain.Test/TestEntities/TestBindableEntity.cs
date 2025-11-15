namespace A5Soft.CARMA.Domain.Test.TestEntities
{
    /// <summary>
    /// Test entity for BindableBase tests
    /// </summary>
    public class TestBindableEntity : BindableBase
    {
        private string _name = string.Empty;
        private int _age;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    OnPropertyChanging(nameof(Name));
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if (_age != value)
                {
                    OnPropertyChanging(nameof(Age));
                    _age = value;
                    OnPropertyChanged(nameof(Age));
                }
            }
        }

        // Expose protected methods for testing
        public void TestOnPropertyChanged(string propertyName)
            => OnPropertyChanged(propertyName);

        public void TestOnPropertiesChanged(params string[] propertyNames)
            => OnPropertiesChanged(propertyNames);

        public void TestOnPropertyChanging(string propertyName)
            => OnPropertyChanging(propertyName);
    }
}

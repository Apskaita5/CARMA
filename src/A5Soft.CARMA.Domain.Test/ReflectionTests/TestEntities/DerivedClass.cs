namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    [Test("Derived")]
    class DerivedClass : BaseClass
    {
        [Test("DerivedProp")]
        public override string Property1 { get; set; }
        public string Property3 { get; set; }
    }
}

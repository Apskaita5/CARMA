namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    [Test("Base")]
    class BaseClass
    {
        [Test("BaseProp")]
        public virtual string Property1 { get; set; }
        public string Property2 { get; set; }
    }
}

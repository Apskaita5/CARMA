namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    [Test("Implementation")]
    class ImplementationClass : BaseClass, ITestInterface1, ITestInterface2
    {
        public string Property2 { get; set; }
    }
}

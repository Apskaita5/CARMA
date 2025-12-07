namespace A5Soft.CARMA.Domain.Test.ReflectionTests.TestEntities
{
    [Test("Interface2")]
    interface ITestInterface2 : ITestInterface1
    {
        string Property2 { get; set; }
    }
}

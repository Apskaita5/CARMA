using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// An attribute to mark a serialization constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class SerializationConstructorAttribute : Attribute
    {
    }
}

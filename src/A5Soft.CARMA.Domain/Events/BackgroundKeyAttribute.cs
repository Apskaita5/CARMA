using System;

namespace A5Soft.CARMA.Domain.Events
{
    /// <summary>
    /// Gives an application event type, an event handler or a cron job its stable key: the name it is stored and
    /// configured under, independent of the class name, its namespace and its assembly.
    /// </summary>
    /// <remarks>
    /// <para>Required on every concrete <see cref="IAppEvent"/> type, every event handler and every cron job: the
    /// startup scan refuses one without it, and a key used twice within its kind (events, handlers, cron jobs).</para>
    /// <para>Never change a key once work items are stored under it: they would no longer find their type. Renaming
    /// the class is safe. A plugin should prefix its keys (e.g. with its plugin identifier).</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BackgroundKeyAttribute : Attribute
    {
        /// <summary>
        /// Gives the class its stable key.
        /// </summary>
        /// <param name="key">the key, e.g. "claims.approved"; compared ignoring case</param>
        public BackgroundKeyAttribute(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            Key = key.Trim();
        }


        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key { get; }
    }
}

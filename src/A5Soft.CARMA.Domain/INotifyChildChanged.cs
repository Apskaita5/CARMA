using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Implemented by classes that notify when a child object has changed.
    /// </summary>
    public interface INotifyChildChanged
    {
        /// <summary>
        /// Event indicating that a child object has changed.
        /// </summary>
        event EventHandler<ChildChangedEventArgs> ChildChanged;
    }
}

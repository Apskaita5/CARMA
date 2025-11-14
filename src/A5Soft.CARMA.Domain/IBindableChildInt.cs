namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Internal interface for propagating <see cref="IBindable"/> options.
    /// </summary>
    internal interface IBindableChildInt
    {
        /// <summary>
        /// Updates <see cref="IBindable"/> options to match parent.
        /// </summary>
        /// <param name="parent">a parent to copy <see cref="IBindable"/> options from</param>
        void UpdateBindingOptions(IBindable parent);
    }
}

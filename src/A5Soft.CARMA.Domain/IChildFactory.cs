using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Implement to enable <see cref="DomainBindingList{TChild}"/> to add new child items without inheritance,
    /// 'see <see cref="DomainBindingList{TChild}.ChildFactory"/>.
    /// </summary>
    /// <typeparam name="TChild">a type of the child items that the factory can create</typeparam>
    public interface IChildFactory<TChild>
    {
        /// <summary>
        /// creates a new child item
        /// </summary>
        /// <param name="parent">a parent of the list (if any)</param>
        /// <param name="validationEngineProvider">a validation engine provider for DI</param>
        TChild CreateNew(IDomainObject parent, IValidationEngineProvider validationEngineProvider);
    }
}

namespace A5Soft.CARMA.Domain.Internal
{
    interface IChildInternal : IChild
    {
        void SetParent(IDomainObject parent, string parentPropertyName);
    }
}

namespace A5Soft.CARMA.Domain
{
    interface IChildInternal : IChild
    {
        void SetParent(IDomainObject parent, string parentPropertyName);
    }
}

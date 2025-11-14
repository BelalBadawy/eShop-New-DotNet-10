namespace eShop.Domain.Events
{
    public class BrandCreatedEvent : IDomainEvent
    {
        public int BrandId { get; }
        public BrandCreatedEvent(int id)
        {
            BrandId = id;
        }
    }
}

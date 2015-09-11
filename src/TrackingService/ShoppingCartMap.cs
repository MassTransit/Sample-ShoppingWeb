namespace TrackingService
{
    using CartTracking;
    using MassTransit.EntityFrameworkIntegration;


    public class ShoppingCartMap :
        SagaClassMapping<ShoppingCart>
    {
        public ShoppingCartMap()
        {
            Property(x => x.CurrentState)
                .HasMaxLength(64);

            Property(x => x.Created);
            Property(x => x.Updated);

            Property(x => x.UserName)
                .HasMaxLength(256);

            Property(x => x.ExpirationId);
            Property(x => x.OrderId);
        }
    }
}
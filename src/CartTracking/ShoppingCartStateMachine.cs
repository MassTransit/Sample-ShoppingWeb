namespace CartTracking
{
    using System;
    using Automatonymous;
    using Shopping.Contracts;


    public class ShoppingCartStateMachine :
        MassTransitStateMachine<ShoppingCart>
    {
        public ShoppingCartStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => ItemAdded, x => x.CorrelateBy(cart => cart.UserName, context => context.Message.UserName)
                .SelectId(context => Guid.NewGuid()));

            Event(() => Submitted, x => x.CorrelateById(context => context.Message.CartId));

            Schedule(() => CartExpired, x => x.ExpirationId, x =>
            {
                x.Delay = TimeSpan.FromSeconds(10);
                x.Received = e => e.CorrelateById(context => context.Message.CartId);
            });

            Initially(
                When(ItemAdded)
                    .Then(context =>
                    {
                        context.Instance.Created = context.Data.Timestamp;
                        context.Instance.Updated = context.Data.Timestamp;
                        context.Instance.UserName = context.Data.UserName;
                    })
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Item Added: {context.Data.UserName} to {context.Instance.CorrelationId}"))
                    .Schedule(CartExpired, context => new CartExpiredEvent(context.Instance))
                    .TransitionTo(Active)
                );

            During(Active,
                When(Submitted)
                    .Then(context =>
                    {
                        if (context.Data.Timestamp > context.Instance.Updated)
                            context.Instance.Updated = context.Data.Timestamp;

                        context.Instance.OrderId = context.Data.OrderId;
                    })
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Cart Submitted: {context.Data.UserName} to {context.Instance.CorrelationId}"))
                    .Unschedule(CartExpired)
                    .TransitionTo(Ordered),
                When(ItemAdded)
                    .Then(context =>
                    {
                        if (context.Data.Timestamp > context.Instance.Updated)
                            context.Instance.Updated = context.Data.Timestamp;
                    })
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Item Added: {context.Data.UserName} to {context.Instance.CorrelationId}"))
                    .Schedule(CartExpired, context => new CartExpiredEvent(context.Instance)),
                When(CartExpired.Received)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Item Expired: {context.Instance.CorrelationId}"))
                    .Publish(context => new CartRemovedEvent(context.Instance))
                    .Finalize()
                );

            SetCompletedWhenFinalized();
        }


        public State Active { get; private set; }
        public State Ordered { get; private set; }

        public Schedule<ShoppingCart, CartExpired> CartExpired { get; private set; }

        public Event<CartItemAdded> ItemAdded { get; private set; }
        public Event<OrderSubmitted> Submitted { get; private set; }


        class CartExpiredEvent :
            CartExpired
        {
            readonly ShoppingCart _instance;

            public CartExpiredEvent(ShoppingCart instance)
            {
                _instance = instance;
            }

            public Guid CartId => _instance.CorrelationId;
        }


        class CartRemovedEvent :
            CartRemoved
        {
            readonly ShoppingCart _instance;

            public CartRemovedEvent(ShoppingCart instance)
            {
                _instance = instance;
            }

            public Guid CartId => _instance.CorrelationId;
            public string UserName => _instance.UserName;
        }
    }
}
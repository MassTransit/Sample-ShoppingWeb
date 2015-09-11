namespace CartTracking
{
    using System;
    using Automatonymous;


    public class ShoppingCart :
        SagaStateMachineInstance
    {
        public string CurrentState { get; set; }

        public string UserName { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        /// <summary>
        /// The expiration tag for the shopping cart, which is scheduled whenever
        /// the cart is updated
        /// </summary>
        public Guid? ExpirationId { get; set; }

        public Guid? OrderId { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
namespace Shopping.Contracts
{
    using System;


    public interface CartItemAdded
    {
        DateTime Timestamp { get; }

        string UserName { get; }
    }
}
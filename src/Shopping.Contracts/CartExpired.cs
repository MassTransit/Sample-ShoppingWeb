namespace Shopping.Contracts
{
    using System;


    public interface CartExpired
    {
        Guid CartId { get; }
    }
}
namespace Shopping.Contracts
{
    using System;


    public interface CartRemoved
    {
        Guid CartId { get; }
        string UserName { get; }
    }
}
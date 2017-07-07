using System;
using NServiceBus;

public class CancelOrder :
    IMessage
{
    public Guid OrderId { get; set; }
}
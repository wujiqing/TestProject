using System;
using Test.Domain.Abstractions;
using Test.Ordering.Domain.OrderAggregate;

namespace Test.Ordering.Domain.Events
{
    public class OrderCreatedDomainEvent:IDomainEvent
    {
        public Order Order { get; private set; }
        public OrderCreatedDomainEvent(Order order)
        {
            this.Order = order;
        }
    }
}

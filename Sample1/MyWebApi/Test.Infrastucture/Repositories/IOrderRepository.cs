using System;
using System.Collections.Generic;
using System.Text;
using Test.Infrastructure.core;
using Test.Ordering.Domain.Events;
using Test.Ordering.Domain.OrderAggregate;

namespace Test.Infrastucture.Repositories
{
    public interface IOrderRepository: IRepository<Order,long>
    {
    }
}

using Microsoft.Extensions.Logging;
using System;
using Test.Infrastructure.core.Behaviors;

namespace Test.Infrastucture
{
    public class OrderingContextTransactionBehavior<TRequest, TResponse> : TransactionBehavior<OrderingContext, TRequest, TResponse>
    {
        public OrderingContextTransactionBehavior(OrderingContext dbContext, ILogger<OrderingContextTransactionBehavior<TRequest, TResponse>> logger) : base(dbContext, logger)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Application.IntegrationEvents
{
    public class OrderPaymentSucceededIntegrationEvent
    {
        public OrderPaymentSucceededIntegrationEvent(long orderId) => OrderId = orderId;
        public long OrderId { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Application.IntegrationEvents
{
    public interface ISubscriberService
    {
        void OrderPaymentSucceeded(OrderPaymentSucceededIntegrationEvent @event);
    }
}

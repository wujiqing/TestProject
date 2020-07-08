using DotNetCore.CAP;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Application.IntegrationEvents
{
    public class SubscriberService : ISubscriberService, ICapSubscribe
    {
        IMediator _mediator;
        public SubscriberService(IMediator mediator)
        {
            _mediator = mediator;
        }

        [CapSubscribe("OrderPaymentSucceeded")]
        public void OrderPaymentSucceeded(OrderPaymentSucceededIntegrationEvent @event)
        {
             
        }

        [CapSubscribe("OrderCreated")]
        public void OrderCreated(OrderCreatedIntegrationEvent @event)
        {

        }
    }
}

using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Domain.Abstractions
{
    public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent: IDomainEvent
    {
    }
}

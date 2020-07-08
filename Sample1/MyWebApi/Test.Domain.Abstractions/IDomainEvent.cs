using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Domain.Abstractions
{
    public interface IDomainEvent: INotification
    {
    }
}

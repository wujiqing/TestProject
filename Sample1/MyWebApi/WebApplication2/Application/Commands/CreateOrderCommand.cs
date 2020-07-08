using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Application.Commands
{
    public class CreateOrderCommand:IRequest<long>
    {
        public CreateOrderCommand(int itemCount)
        {
            ItemCount = itemCount;
        }

        public long ItemCount { get; private set; }
    }
}

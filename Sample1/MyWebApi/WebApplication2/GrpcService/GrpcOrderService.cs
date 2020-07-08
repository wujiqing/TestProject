using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using TestOrderGrpcServices;

namespace WebApplication2.GrpcService
{
    public class OrderService : OrderGrpc.OrderGrpcBase
    {
        public OrderService() { }

        public override Task<CreateOrderResult> CreateOrder(CreateOrderCommand request, ServerCallContext context)
        { 
            //添加创建订单的内部逻辑，录入将订单信息存储到数据库
            return Task.FromResult(new CreateOrderResult { OrderId = 24 });
        }
    }
}

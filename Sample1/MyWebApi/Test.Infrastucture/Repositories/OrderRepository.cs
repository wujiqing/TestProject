using System;
using System.Collections.Generic;
using System.Text;
using Test.Infrastructure.core;
using Test.Ordering.Domain.Events;
using Test.Ordering.Domain.OrderAggregate;

namespace Test.Infrastucture.Repositories
{
    public class OrderRepository : Repository<Order, long, OrderingContext>, IOrderRepository
    {
        //Repository作为泛型类，数据库上下文对象作为泛型变量通过构建函数传递进来，实现一系列增删改方法；
        //OrderRepository则相当于某个服务，比以往省掉传泛型类型，照此理推算每张表对应一服务？？ 
        //搞个表对应的服务则需要：表仓储接口，表仓储具体实现服务，数据库上下文（作用利用OrderRepository创建表，几个表对象可以共用一个）
        //Repository中数据库上下文中DbSet是何用意？Repository中已经提供增删改，DbSet作为属性也没注入
        //IUnitOfWork 相当于上下文中一部分，故在Repository中对外公开一部分属性

        //命令查询职责分离模式，请求=》发送到请求处理器=》当领域模型发生变更时，触发推送领域事件=》
        //接收到通知，各个回调处理器进行推送到集成事件总线，进行订阅处理？为何不接收到通知就处理，处理耗时；
        //为何不直接在接收到命令请求时，直接发送到事件总线？？事件很多情况下，应交给公共方法统一去处理，如果辨别各事件，这里采用各事件回调方式处理；然后进行发布到事件总线
        
        public OrderRepository(OrderingContext context) : base(context)
        {
        } 
    }
}

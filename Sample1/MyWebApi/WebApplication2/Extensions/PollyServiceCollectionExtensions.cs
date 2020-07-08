using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Test.Infrastucture;
using Test.Infrastucture.Repositories;
using Test.Ordering.Domain.Events;
using Test.Ordering.Domain.OrderAggregate;
using WebApplication2.Application.IntegrationEvents;

namespace WebApplication2.Extensions
{
    public static class PollyServiceCollectionExtensions
    {
        static void GetRetryPolicy()
        {
            //如有异常，重试3次
            Policy.Handle<RpcException>().Retry(retryCount: 3, onRetry: (ex, t, context) => { });

            //
            Policy.Handle<RpcException>().RetryForever(onRetry: (ex, t, context) => { });

            Policy.Handle<RpcException>().WaitAndRetry(new[] {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            });

            //如有异常，隔2的次数次方执行3次
            Policy.Handle<RpcException>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            Policy.Handle<RpcException>().WaitAndRetry(retryCount: 10, sleepDurationProvider: t => TimeSpan.FromSeconds(t), onRetry: (ex, t, c) => { });

        }

        static void GetTimeOutPolicy()
        {
            //超时，如执行中重试到20秒则中断
            Policy.Timeout(20);

            Policy.Timeout(20, onTimeout: (context, ts, task) => { });

            //悲观超时,不取消超时任务
            Policy.Timeout(20, Polly.Timeout.TimeoutStrategy.Pessimistic);
        }

        //熔断
        static void GetCircuitBreakerPolicy()
        {
            var cancellationToken = new CancellationToken();

            //出现3次异常则熔断，熔断指的是？？
            Policy.Handle<RpcException>().CircuitBreaker(exceptionsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(20));

            //出现3次异常则熔断
            Policy.Handle<RpcException>().CircuitBreaker(exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(20),
                onBreak: (ex, state, ts, context) => { },
                onReset: context => { },
                onHalfOpen: () => { });

            Policy.Handle<RpcException>().AdvancedCircuitBreaker(failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(5),
                minimumThroughput: 100,
                durationOfBreak: TimeSpan.FromSeconds(20),
                onBreak: (ex, state, ts, context) => { },
                onReset: context => { },
                onHalfOpen: () => { });
        }

        //舱壁隔离
        static void GetBulkheadPolicy()
        {
            //仅允许一定界限的执行
            Policy.Bulkhead(maxParallelization: 5, maxQueuingActions: 10, onBulkheadRejected: context => { }).Execute(() => { });
        }

        static IAsyncPolicy<HttpResponseMessage> PolicyWrap()
        {
            var defaultMessage = new HttpResponseMessage(HttpStatusCode.OK);
            defaultMessage.Content = new StringContent("{}");

            var bcFallback = Policy<HttpResponseMessage>.Handle<BrokenCircuitException>().FallbackAsync<HttpResponseMessage>(defaultMessage);

            var retry = Policy<HttpResponseMessage>.Handle<Exception>().WaitAndRetryAsync(3,t=>TimeSpan.FromSeconds(t+1));

            var bc=Policy<HttpResponseMessage>.Handle<Exception>().AdvancedCircuitBreakerAsync(failureThreshold:0.5,
                samplingDuration:TimeSpan.FromSeconds(5),
                minimumThroughput:100,
                durationOfBreak:TimeSpan.FromSeconds(20),
                onBreak: (ex, state, ts, context) => { },
                onReset: context => { },
                onHalfOpen: () => { });

            //组合重试3次，并如达到一半，则熔断20秒
            return Policy.WrapAsync(retry, bcFallback, bc);

        }
    }
}

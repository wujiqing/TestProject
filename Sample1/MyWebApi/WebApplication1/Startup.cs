using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using TestOrderGrpcServices;
using static TestOrderGrpcServices.OrderGrpc;

namespace WebApplication1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true); //允许使用不加密的HTTP/2协议
            services.AddGrpcClient<OrderGrpc.OrderGrpcClient>(options =>
            {
                //服务器的grpc的服务已经实现，客户访问需要协议 TSL/SSL ？？http2
                //如服务端http不是http2加密的，则客户端需请求时允许未加密请求
                //使用证书，如证书不是正规经过检验的证书，则需客户端处理请求时去掉证书检验
                //在启动json配置文件中profiles，配置IIS Express起启项设置会影响到GRPC http2请求？？
                options.Address = new Uri("http://localhost:5002");
            }).ConfigurePrimaryHttpMessageHandler(provider => {
                var handler = new SocketsHttpHandler();
                handler.SslOptions.RemoteCertificateValidationCallback = (a, b, c, d) => true;
                return handler;
            }).AddTransientHttpErrorPolicy(p => p.WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(i * 3)));

            //var reg = services.AddPolicyRegistry();

            //reg.Add("retryforever", Policy.HandleResult<HttpResponseMessage>(message =>
            //{
            //    return message.StatusCode == System.Net.HttpStatusCode.Created;
            //}).RetryForeverAsync());

            //services.AddHttpClient("orderclient").AddPolicyHandlerFromRegistry("retryforever");

            //services.AddHttpClient("orderclientv2").AddPolicyHandlerFromRegistry((r, m) =>
            //{
            //    return m.Method == HttpMethod.Get ? r.Get<IAsyncPolicy<HttpResponseMessage>>("retryforever") : Policy.NoOpAsync<HttpResponseMessage>();
            //});

            //services.AddHttpClient("orderclientv3").AddPolicyHandler(Policy<HttpResponseMessage>.Handle<HttpRequestException>().CircuitBreakerAsync(
            //     handledEventsAllowedBeforeBreaking: 10,
            //    durationOfBreak: TimeSpan.FromSeconds(10),
            //    onBreak: (r, t) => { },
            //    onReset: () => { },
            //    onHalfOpen: () => { }
            //    ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //重试策略：可设置任意时间间隔，间隔指从执行完成跟错误回调执行完成开始算起
            //熔断：配合重试策略，当异常次数与吞吐量占比达到一定程度则熔断该服务
            //回退；超时：通过token，传递超时状态，并不会抛出异常；隔离：当达到一定并发量，或单个任务队列中达到设定值时，会异常

            var retryPolicy = Policy.Handle<Exception>()
                                .WaitAndRetry(retryCount: 5, i => TimeSpan.FromSeconds(3),
                                //每次错误的回调方法
                                (exception, timespan) =>
                                {
                                    Console.WriteLine($"===========记录重试失败回调开始{DateTime.Now}===========");
                                    //Thread.Sleep(2000);
                                    //Console.WriteLine($"===========记录重试失败回调2秒结束{DateTime.Now}===========");
                                });

            Console.WriteLine($"===========重试下步骤代码===========");

            //如果当前连续有两个异常，那么触发熔断，10s内不能调用，10s之后重新调用。
            //一旦调用成功了，熔断就解除了。
            //var ciruitPolicy = Policy.Handle<Exception>()
            //                        //触发熔断的异次数,熔断的时间间隔(秒)         触发熔断的回调方法
            //                        .CircuitBreaker(2, TimeSpan.FromSeconds(3), (ex, timespan, context) =>
            //                        {
            //                            //触发熔断
            //                            Console.WriteLine($"{DateTime.Now} 熔断触发：{timespan}");
            //                        }, (context) =>
            //                        {
            //                            //恢复熔断
            //                            Console.WriteLine($"{DateTime.Now} 熔断恢复");
            //                        }, onHalfOpen: () =>
            //                        {
            //                            Console.WriteLine("onHalfOpen");
            //                        });

            //////try
            //////{
            //var ciruitPolicyCount = 0;
            //Policy.Wrap(retryPolicy, ciruitPolicy).Execute((context) =>
            //{
            //    Console.WriteLine($"===========记录重试中开始{DateTime.Now}===========");
            //    //Thread.Sleep(2000);
            //    //Console.WriteLine($"===========记录重试中结束{DateTime.Now}===========");
            //    ciruitPolicyCount++;
            //    if (ciruitPolicyCount<3)
            //    { 
            //    throw new Exception("重试");
            //    }
            //}, new Dictionary<string, object>() { { "1", "a" } });

            //    Console.WriteLine("=======熔断下步代码=======");
            //}
            //catch (Exception ex) //熔断触发后，在熔断时间内都执行以下下逻辑
            //{
            //    Console.WriteLine(string.Format("{0} 业务逻辑异常：'{1}'", DateTime.Now, ex.Message));
            //}

            //var policy = Policy.Timeout(3);
            //var token = new CancellationToken();

            //policy.Execute(token =>
            //{
            //    for (var i = 0; i < 10; i++)
            //    {
            //        Console.WriteLine($"时间：{DateTime.Now},token.IsCancellationRequested:{token.IsCancellationRequested}" );
            //        Thread.Sleep(1000);
            //    }
            //},token);

            //并发数：线程数，每线程里的队列堆积
            //var policy = Policy.Bulkhead(5);
            //var count = 0;
            //var list = new List<string>();
            //Parallel.For(0, 60, i =>
            //{
            //    var result = policy.Execute<string>(() =>
            //    { 
            //        count++;
            //        list.Add(Thread.CurrentThread.ManagedThreadId.ToString());
            //        return "index:"+ i+",threadID:"+Thread.CurrentThread.ManagedThreadId+",总数："+ count;
            //    });

            //    Console.WriteLine("已成功获取到数据:{0}", result);
            //});

            //var listCount = list.Distinct().Count();


            app.UseEndpoints(endpoints =>
                                                { 

                                                    endpoints.MapGet("/", async context =>
                                                    {
                                            //var service = context.RequestServices.GetService<OrderGrpcClient>();

                                            //try
                                            //{ 
                                            //    var r = service.CreateOrder(new CreateOrderCommand { BuyerId = "abc" });
                                            //}
                                            //catch (Exception ex)
                                            //{

                                            //}

                                            await context.Response.WriteAsync("Hello World!");
                                                    });
                                                });
        }
    }
}

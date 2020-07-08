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
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true); //����ʹ�ò����ܵ�HTTP/2Э��
            services.AddGrpcClient<OrderGrpc.OrderGrpcClient>(options =>
            {
                //��������grpc�ķ����Ѿ�ʵ�֣��ͻ�������ҪЭ�� TSL/SSL ����http2
                //������http����http2���ܵģ���ͻ���������ʱ����δ��������
                //ʹ��֤�飬��֤�鲻�����澭�������֤�飬����ͻ��˴�������ʱȥ��֤�����
                //������json�����ļ���profiles������IIS Express���������û�Ӱ�쵽GRPC http2���󣿣�
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

            //���Բ��ԣ�����������ʱ���������ָ��ִ����ɸ�����ص�ִ����ɿ�ʼ����
            //�۶ϣ�������Բ��ԣ����쳣������������ռ�ȴﵽһ���̶����۶ϸ÷���
            //���ˣ���ʱ��ͨ��token�����ݳ�ʱ״̬���������׳��쳣�����룺���ﵽһ�����������򵥸���������дﵽ�趨ֵʱ�����쳣

            var retryPolicy = Policy.Handle<Exception>()
                                .WaitAndRetry(retryCount: 5, i => TimeSpan.FromSeconds(3),
                                //ÿ�δ���Ļص�����
                                (exception, timespan) =>
                                {
                                    Console.WriteLine($"===========��¼����ʧ�ܻص���ʼ{DateTime.Now}===========");
                                    //Thread.Sleep(2000);
                                    //Console.WriteLine($"===========��¼����ʧ�ܻص�2�����{DateTime.Now}===========");
                                });

            Console.WriteLine($"===========�����²������===========");

            //�����ǰ�����������쳣����ô�����۶ϣ�10s�ڲ��ܵ��ã�10s֮�����µ��á�
            //һ�����óɹ��ˣ��۶Ͼͽ���ˡ�
            //var ciruitPolicy = Policy.Handle<Exception>()
            //                        //�����۶ϵ������,�۶ϵ�ʱ����(��)         �����۶ϵĻص�����
            //                        .CircuitBreaker(2, TimeSpan.FromSeconds(3), (ex, timespan, context) =>
            //                        {
            //                            //�����۶�
            //                            Console.WriteLine($"{DateTime.Now} �۶ϴ�����{timespan}");
            //                        }, (context) =>
            //                        {
            //                            //�ָ��۶�
            //                            Console.WriteLine($"{DateTime.Now} �۶ϻָ�");
            //                        }, onHalfOpen: () =>
            //                        {
            //                            Console.WriteLine("onHalfOpen");
            //                        });

            //////try
            //////{
            //var ciruitPolicyCount = 0;
            //Policy.Wrap(retryPolicy, ciruitPolicy).Execute((context) =>
            //{
            //    Console.WriteLine($"===========��¼�����п�ʼ{DateTime.Now}===========");
            //    //Thread.Sleep(2000);
            //    //Console.WriteLine($"===========��¼�����н���{DateTime.Now}===========");
            //    ciruitPolicyCount++;
            //    if (ciruitPolicyCount<3)
            //    { 
            //    throw new Exception("����");
            //    }
            //}, new Dictionary<string, object>() { { "1", "a" } });

            //    Console.WriteLine("=======�۶��²�����=======");
            //}
            //catch (Exception ex) //�۶ϴ��������۶�ʱ���ڶ�ִ���������߼�
            //{
            //    Console.WriteLine(string.Format("{0} ҵ���߼��쳣��'{1}'", DateTime.Now, ex.Message));
            //}

            //var policy = Policy.Timeout(3);
            //var token = new CancellationToken();

            //policy.Execute(token =>
            //{
            //    for (var i = 0; i < 10; i++)
            //    {
            //        Console.WriteLine($"ʱ�䣺{DateTime.Now},token.IsCancellationRequested:{token.IsCancellationRequested}" );
            //        Thread.Sleep(1000);
            //    }
            //},token);

            //���������߳�����ÿ�߳���Ķ��жѻ�
            //var policy = Policy.Bulkhead(5);
            //var count = 0;
            //var list = new List<string>();
            //Parallel.For(0, 60, i =>
            //{
            //    var result = policy.Execute<string>(() =>
            //    { 
            //        count++;
            //        list.Add(Thread.CurrentThread.ManagedThreadId.ToString());
            //        return "index:"+ i+",threadID:"+Thread.CurrentThread.ManagedThreadId+",������"+ count;
            //    });

            //    Console.WriteLine("�ѳɹ���ȡ������:{0}", result);
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

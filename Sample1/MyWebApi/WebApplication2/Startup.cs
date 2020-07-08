using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Test.Infrastucture;
using WebApplication2.ClientApi;
using WebApplication2.ClientApi.DelegatingHandler;
using WebApplication2.Extensions;
using WebApplication2.GrpcService;
using WebApplication2.Intercreptors;

namespace WebApplication2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(); //支持构造函数序列化
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            Console.WriteLine("ConfigureServices:");

            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = false;
                options.Interceptors.Add<ExceptionInterceptor>();
            });

            services.AddControllers(); 
            services.AddMediatRServices(); 
            services.AddMySqlDomainContext(Configuration.GetValue<string>("Mysql")); 
            services.AddRepositories();
            services.AddEventBus(Configuration);

            //请求管道：可在请求作处理后再返回，但结果与写个扩展方法，扩展方法再作特殊处理，控制与实现效果相差不大;
            services.AddSingleton<RequestIdDelegatingHandler>();
            services.AddHttpClient<TypedOrderServiceClient>().AddHttpMessageHandler(provider => provider.GetService<RequestIdDelegatingHandler>());

            var logginConfig = Configuration.GetSection("Logging");
            services.AddSingleton(p => Configuration);
            services.AddLogging(builer =>
            {
                builer.AddConfiguration(Configuration.GetSection("Logging"));
                builer.AddConsole();
                builer.AddDebug();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //TODO
            using (var scope = app.ApplicationServices.CreateScope())
            {
                //var dc = scope.ServiceProvider.GetService<OrderingContext>();
                //Console.WriteLine("Database.EnsureCreated:开始");
                //var isCreated= dc.Database.EnsureCreated();
                 
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
             
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<OrderService>();
            });

        }
    }
}

using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileCreateWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {

            services.AddDbContext<NORTHWNDContext>(options =>
            {
                options.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlServer"));
            });


            services.AddSingleton<RabbitMQClientService>();
            //IConfiguration configuration = hostContext.Configuration;
            services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(hostContext.Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true });
            services.AddHostedService<Worker>();
        });
    }
}

﻿using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project.Common.Services;
using Project.Events;
using Project.Events.ServiceBus;

namespace Project.EventProcessor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddAzureClients(builder =>
                        {
                            builder.AddServiceBusClient(hostContext.Configuration.GetConnectionString("ServiceBus"));
                        });

                        services.AddSingleton<IEventListener, ServiceBusTopicEventListener>();
                        services.AddSingleton<IEventHandler, PlacedOrderEventHandler>();
                        services.AddSingleton<IEventHandler, ModifiedOrderEventHandler>();

                        services.AddHostedService<PlaceOrderWorker>();
                        services.AddHostedService<ModifyOrderWorker>();
                    });
    }
}
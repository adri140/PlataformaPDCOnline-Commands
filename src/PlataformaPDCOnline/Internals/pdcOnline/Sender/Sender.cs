﻿using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pdc.Hosting;
using Pdc.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace PlataformaPDCOnline.Internals.pdcOnline.Sender
{
    //contiene todos los datos para poder crear el sender, usamos singelton para que solo se pueda instanciar una vez
    public class Sender
    {

        private static Sender CommandsSender;

        private readonly IConfiguration configuration;
        private readonly IServiceProvider services;
        private SqliteConnection connection;
        private IServiceScope scope;
        private IHostedService boundedContext;
        private ICommandSender sender;

        public Sender()
        {
            configuration = GetConfiguration();

            services = GetBoundedContextServices();

            InicializeAsync();
        }

        private async Task InicializeAsync()
        {
            using (scope)
            {

                boundedContext = services.GetRequiredService<IHostedService>();

                await boundedContext.StartAsync(default); //iniciamos todos los servicios

                sender = services.GetRequiredService<ICommandSender>();
            }
        }

        public async Task SendCommandAsync(Command command)
        {
            if (command != null)
            {
                using (scope)
                {
                    await sender.SendAsync(command);
                    Console.WriteLine("enviando command");
                }
            }
        }

        public async Task EndJobAsync()
        {
            using (scope)
            {
            if (boundedContext != null)
            {
                await boundedContext.StopAsync(default);
            }
            }
        }

        private static IConfiguration GetConfiguration()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var c = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "DistributedRedisCache:InstanceName", "Cache." },
                    { "RedisDistributedLocks:InstanceName", "Locks." },
                    { "DocumentDBPersistence:Database", "Tests" },
                    { "DocumentDBPersistence:Collection", "Events" },
                    { "ProcessManager:Sender:EntityPath", "core-test-commands" },
                    { "BoundedContext:Publisher:EntityPath", "core-test-events" },
                    { "CommandHandler:Receiver:EntityPath", "core-test-commands" },
                    { "Denormalization:Subscribers:0:EntityPath", "core-test-events" },
                    { "Denormalization:Subscribers:0:SubscriptionName", "core-test-events-denormalizers" }
                })
                .AddUserSecrets(assembly, optional: true)
                .AddEnvironmentVariables()
                .Build();

#if !DEBUG
            return new ConfigurationBuilder()
                .AddConfiguration(c)
                .AddAzureKeyVault(c["AzureKeyVault:Uri"], c["AzureKeyVault:ClientId"], c["AzureKeyVault:ClientSecret"])
                .Build();
#else
            return c;
#endif
        }

        private IServiceProvider GetBoundedContextServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.AddDebug());

            //enviar un command
            services.AddAzureServiceBusCommandSender(options => configuration.GetSection("ProcessManager:Sender").Bind(options));
            //fin command sender

            //esto es necesario siempre, no lo toques o moriras
            services.AddHostedService<HostedService>();

            return services.BuildServiceProvider();
        }
    }
}

﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pdc.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PlataformaPDCOnline.Internals.pdcOnline.Sender
{
    public class PrepareSender
    {
        private static PrepareSender prepare = null;

        public static ICommandSender Singelton()
        {
            if(prepare == null)
            {
                prepare = new PrepareSender();
            }
            return prepare.sender;
        }

        private readonly IConfiguration configuration;
        public ICommandSender sender;

        private PrepareSender()
        {
            configuration = GetConfiguration();
            this.sender = GetProcessManagerServices().GetRequiredService<ICommandSender>();
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

        private IServiceProvider GetProcessManagerServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.AddDebug());

            services.AddAzureServiceBusCommandSender(options => options.Bind(configuration.GetSection("ProcessManager:Sender")));

            return services.BuildServiceProvider();
        }
    }
}

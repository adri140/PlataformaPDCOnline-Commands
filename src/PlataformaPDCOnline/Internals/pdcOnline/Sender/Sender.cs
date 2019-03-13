

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pdc.Messaging;
using PlataformaPDCOnline.Editable.pdcOnline.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PlataformaPDCOnline.Internals.pdcOnline.Sender
{
    //contiene todos los datos para poder crear el sender, usamos singelton para que solo se pueda instanciar una vez
    public class Sender
    {
        private readonly IConfiguration configuration;
        private ICommandSender sender;
        private Task boundedContextWorker;
        private CancellationTokenSource cancellationTokenSource;

        public Sender()
        {
            configuration = GetConfiguration();
            sender = GetProcessManagerServices().GetRequiredService<ICommandSender>();
            cancellationTokenSource = new CancellationTokenSource();

            //temporal, recive los commands y crea los eventos, despues los reenvia, creo
            var boundedContextServices = GetBoundedContextServices();
            boundedContextWorker = ExecuteBoundedContextAsync(boundedContextServices, cancellationTokenSource.Token);
        }

        public async void sendAsync(Command command)
        {
            await sender.SendAsync(command);
        }

        //temporal
        public void end()
        {
            Console.WriteLine("end");
            cancellationTokenSource.Cancel();
            Thread.Sleep(2000);
        }

        private IServiceProvider GetProcessManagerServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.AddDebug());

            services.AddAzureServiceBusCommandSender(options => options.Bind(configuration.GetSection("ProcessManager:Sender"))); //configura la suscripción a la que se envian los commands

            return services.BuildServiceProvider();
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

        //temporal
        private IServiceProvider GetBoundedContextServices()
        {
            var services = new ServiceCollection();

            services.AddCommandHandler(options => options.Bind(configuration.GetSection("CommandHandler")));

            services.AddLogging(builder => builder.AddDebug());
            services.AddAggregateRootFactory();
            services.AddUnitOfWork(options => { });
            services.AddDocumentDBPersistence(options => options.Bind(configuration.GetSection("DocumentDBPersistence")));
            services.AddRedisDistributedLocks(options => options.Bind(configuration.GetSection("RedisDistributedLocks")));
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = configuration["DistributedRedisCache:Configuration"];
                options.InstanceName = configuration["DistributedRedisCache:InstanceName"];
            });
            services.AddAzureServiceBusEventPublisher(options => options.Bind(configuration.GetSection("BoundedContext:Publisher")));

            services.AddCommandHandler<CreateWebUser, CreateWebUserHandler>();
            services.AddCommandHandler<UpdateWebUser, UpdateWebUserHandler>();
            services.AddCommandHandler<DeleteWebUser, DeleteWebUserHandler>();

            return services.BuildServiceProvider();
        }

        //temporal
        private static async Task ExecuteBoundedContextAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            using (var scope = services.CreateScope())
            {
                var boundedContext = services.GetRequiredService<IHostedService>();

                try
                {
                    await boundedContext.StartAsync(default);

                    await Task.Delay(
                        Timeout.InfiniteTimeSpan,
                        cancellationToken);
                }
                catch (TaskCanceledException)
                {

                }
                finally
                {
                    await boundedContext.StopAsync(default);
                }
            }
        }
    }
}

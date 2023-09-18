using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQConsumer.Services;

namespace RabbitMQConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Adding service collection for dependency injection and configuration builder
            ServiceCollection serviceCollection = new ServiceCollection();

            //Reading config from app settings and building it
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            //service collection operations
            serviceCollection.Configure<ApplicationConfigurations>(configuration);
            serviceCollection.AddScoped<IOtpSender, OtpSender>();
            serviceCollection.AddScoped<IUserDetailsService, UserDetailsService>();
            serviceCollection.AddSingleton<RabbitMqManager>();
            serviceCollection.AddSingleton<MessageReceiver>();
            serviceCollection.AddLogging(config => config.AddConsole());

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            //creating the scope to resolve services as background service/ console have no scope as it does in api's
            // in an api application each http request creates a scope
            using(var scope = serviceProvider.CreateScope())
            {
                //get instance of RabbitMqManager and registering queues and exchanges
                var mqManager = scope.ServiceProvider.GetRequiredService<RabbitMqManager>();
                mqManager.RegisterMq();
            }
            Console.ReadLine();
        }

        
    }
}
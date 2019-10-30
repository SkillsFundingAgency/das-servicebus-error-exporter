using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Tools.AnalyseErrorQueues.Engine;
using SFA.DAS.Tools.AnalyseErrorQueues.Services.SvcBusService;
using SFA.DAS.Tools.AnalyseErrorQueues.Services.DataSinkService;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Console
{
    class Program
    {
        async static Task Main(string[] args)
        {
            // Create service collection and configure our services
            var services = ConfigureServices();
            // Generate a provider
            var serviceProvider = services.BuildServiceProvider();
        
            // Kick off our actual code
            await serviceProvider.GetService<QueueAnalyser>().Run();
            System.Console.Read();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            //services.AddTransient<IDataSink, psvDataSink>();
            services.AddTransient<IDataSink, laDataSink>();
            services.AddTransient<ISvcBusService, SvcBusService>();
            
            // Set up the objects we need to get to configuration settings
            var config = LoadConfiguration();
            // Add the config to our DI container for later user
            services.AddSingleton(config);

            // Set up the objects we need to get to configuration settings
            services.AddLogging(logging =>
            {
                logging.AddConfiguration(config.GetSection("Logging"));
                logging.AddConsole();
            });

            // Add the config to our DI container for later user
            services.AddSingleton(config);

            // IMPORTANT! Register our application entry point
            services.AddTransient<QueueAnalyser>();
            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, 
                            reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, 
                            reloadOnChange: true);
            return builder.Build();
        }
    }
}
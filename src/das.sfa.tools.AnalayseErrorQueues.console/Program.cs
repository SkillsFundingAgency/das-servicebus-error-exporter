using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using das.sfa.tools.AnalayseErrorQueues.Engine;
using das.sfa.tools.AnalayseErrorQueues.services.SvcBusService;
using das.sfa.tools.AnalayseErrorQueues.services.DataSinkService;

namespace das.sfa.tools.AnalayseErrorQueues.console
{
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            // Create service collection and configure our services
            var services = ConfigureServices();
            // Generate a provider
            var serviceProvider = services.BuildServiceProvider();
        
            // Kick off our actual code
            var retVal = await serviceProvider.GetService<QueueAnalyser>().Run();
            Console.Read();

            return retVal;
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
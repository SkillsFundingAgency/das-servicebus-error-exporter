using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Tools.JiraDataAnalyser.Domain;
using SFA.DAS.Tools.JiraDataAnalyser.Engine;
using SFA.DAS.Tools.JiraDataAnalyser.Services;
using SFA.DAS.Tools.JiraDataAnalyser.Services.DataSinkService;

namespace SFA.DAS.Tools.JiraDataExporter.console
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
            await serviceProvider.GetService<IAnalyseJira>().Run();
            
            System.Console.WriteLine("Press Enter to End");            
            System.Console.Read();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            //services.AddTransient<IDataSink, psvDataSink>();
            
            // Set up the objects we need to get to configuration settings
            var config = LoadConfiguration();

            // Set up the objects we need to get to configuration settings
            services.AddLogging(logging =>
            {
                logging.AddConfiguration(config.GetSection("Logging"));
                logging.AddConsole();
            });

            // Add the config to our DI container for later user
            services.AddSingleton(config);
            services.Configure<JiraSettings>(opts => config.GetSection("JiraSettings").Bind(opts));
            services.Configure<JiraCredentials>(opts => config.GetSection("JiraCredentials").Bind(opts));
            services.Configure<LogAnalyticsOptions>(opts => config.GetSection("LADataSinkSettings").Bind(opts));
            services.AddTransient<IAnalyseJira, JiraAnalyser>();
            services.AddTransient<IJiraService, JiraService>();
            services.AddTransient<IDataSink, LaDataSink>();
            
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

using DAS.SFA.Tools.AnalyseErrorQueues.Engine;
using DAS.SFA.Tools.AnalyseErrorQueues.Services.DataSinkService;
using DAS.SFA.Tools.AnalyseErrorQueues.Services.SvcBusService;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

[assembly: FunctionsStartup(typeof(DAS.SFA.Tools.AnalyseErrorQueues.Functions.Startup))]

namespace DAS.SFA.Tools.AnalyseErrorQueues.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var executionContextOptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>()
                .Value
            ;
            var appDirectory = executionContextOptions.AppDirectory;
            var config = LoadConfiguration(appDirectory);

            builder.Services.AddTransient(s => new BlobDataSink(config, s.GetRequiredService<ILogger<BlobDataSink>>()));
            builder.Services.AddTransient(s => new laDataSink(config, s.GetRequiredService<ILogger<laDataSink>>()));
            builder.Services.AddTransient<ISvcBusService, SvcBusService>(s =>  new SvcBusService(config, s.GetRequiredService<ILogger<SvcBusService>>()));

            builder.Services.AddTransient<IAnalyseQueues, QueueAnalyser>(s =>
            {
                var sink = s.GetRequiredService<laDataSink>();
                var svc = s.GetRequiredService<ISvcBusService>();
                var log = s.GetRequiredService<ILogger<QueueAnalyser>>();

                return new QueueAnalyser(sink, svc, config, log);
            });

            builder.Services.AddTransient<IAnalyseQueuesBase, QueueAnalyser>(s =>
            {
                var sink = s.GetRequiredService<BlobDataSink>();
                var svc = s.GetRequiredService<ISvcBusService>();
                var log = s.GetRequiredService<ILogger<QueueAnalyser>>();

                return new QueueAnalyser(sink, svc, config, log);
            });
        }

        public static IConfiguration LoadConfiguration(string appDirectory)
        {
            Trace.WriteLine($"appDirectory: {appDirectory}");
            var builder = new ConfigurationBuilder()
                .SetBasePath(appDirectory)
                .AddJsonFile("local.settings.json",
                    optional: true, 
                    reloadOnChange: true)
                .AddJsonFile("appsettings.json",
                    optional: true, 
                    reloadOnChange: true)
                .AddJsonFile("local.appsettings.json",
                    optional: true, 
                    reloadOnChange: true)                    
                ;
            return builder.Build();
        }
    }
}
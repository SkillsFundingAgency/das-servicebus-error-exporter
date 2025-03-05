using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using SFA.DAS.Tools.AnalyseErrorQueues.Engine;
using SFA.DAS.Tools.AnalyseErrorQueues.Services.DataSinkService;
using SFA.DAS.Tools.AnalyseErrorQueues.Services.SvcBusService;
using SFA.DAS.Tools.AnalyseErrorQueues.Domain.Configuration;
using Microsoft.Azure.Functions.Worker.Builder;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static FunctionsApplicationBuilder ConfigureServices(this FunctionsApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            services.Configure<ApplicationConfiguration>(configuration);
            services.Configure<ServiceBusRepoSettings>(configuration.GetSection(nameof(ServiceBusRepoSettings)));
            services.Configure<LADataSinkSettings>(configuration.GetSection(nameof(LADataSinkSettings)));
            services.Configure<BlobDataSinkSettings>(configuration.GetSection(nameof(BlobDataSinkSettings)));

            services.AddLogging();

            services.AddTransient<IDataSink, BlobDataSink>();
            services.AddTransient<ISvcBusService, SvcBusService>();

            services.AddTransient<IAnalyseQueues, QueueAnalyser>(sp =>
            {
                var sink = sp.GetRequiredService<IDataSink>();
                var svc = sp.GetRequiredService<ISvcBusService>();
                var log = sp.GetRequiredService<ILogger<QueueAnalyser>>();
                var serviceBusSettings = sp.GetRequiredService<IOptions<ServiceBusRepoSettings>>();
                return new QueueAnalyser(sink, svc, serviceBusSettings, log);
            });

            services.AddTransient<IAnalyseQueuesBase, QueueAnalyser>(sp =>
            {
                var sink = sp.GetRequiredService<IDataSink>();
                var svc = sp.GetRequiredService<ISvcBusService>();
                var log = sp.GetRequiredService<ILogger<QueueAnalyser>>();
                var serviceBusSettings = sp.GetRequiredService<IOptions<ServiceBusRepoSettings>>();
                return new QueueAnalyser(sink, svc, serviceBusSettings, log);
            });

            builder.Services.AddTransient<ServiceBusClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<ServiceBusRepoSettings>>().Value;
                var configuration = sp.GetRequiredService<IConfiguration>();
                if (configuration["EnvironmentName"] == "LOCAL")
                {
                    return new ServiceBusClient(settings.ServiceBusConnectionString, new ServiceBusClientOptions
                    {
                        TransportType = ServiceBusTransportType.AmqpWebSockets,
                    });
                }
                else
                {
                    return new ServiceBusClient(settings.ServiceBusConnectionString);
                }
            });

            return builder;
        }
    }
}
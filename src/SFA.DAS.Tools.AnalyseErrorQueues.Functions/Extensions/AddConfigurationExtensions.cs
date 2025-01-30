using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using System;
using System.IO;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions.Extensions
{
    public static class ConfigurationExtensions
    {
        public static FunctionsApplicationBuilder AddConfiguration(this FunctionsApplicationBuilder builder)
        {
            var basePath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(basePath, "local.settings.json");

            builder.Configuration
                .SetBasePath(basePath)
                .AddJsonFile(filePath, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.Build();
            var config = builder.Configuration;

            builder.Configuration.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = config["ConfigNames"]?.Split(",") ?? Array.Empty<string>();
                options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
                options.EnvironmentName = config["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            return builder;
        }
    }
}
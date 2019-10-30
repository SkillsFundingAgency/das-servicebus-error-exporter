using SFA.DAS.Tools.AnalyseErrorQueues.Domain;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Services.DataSinkService
{
    public class BlobDataSink : IDataSink
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public BlobDataSink(IConfiguration config, ILogger<BlobDataSink> logger)
        {
            _config = config ?? throw new Exception("config is null");
            _logger = logger ?? throw new Exception("logger is null");
        }

        public void SinkMessages(string envName, string queueName, IEnumerable<sbMessageModel> messages)
        {
            var connString = _config.GetValue<string>("BlobDataSinkSettings:StorageConnectionString");
            var blobContainerName = _config.GetValue<string>("BlobDataSinkSettings:StorageContainerName");

#if DEBUG
            if(_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug($"connString: {connString}");
                _logger.LogDebug($"connString: {blobContainerName}");
            }
#endif

            var sb = new StringBuilder();
            sb.AppendLine("MessageId | TimeOfFailure | ExceptionType | OriginatingEndpoint | ProcessingEndpoint | EnclosedMessageTypes | ExceptionMessage | Stack Trace | Raw");
            foreach (var msg in messages)
            {
                var psvLine = $"{msg.MessageId} |  {msg.TimeOfFailure} | {msg.ExceptionType} | {msg.OriginatingEndpoint} | {msg.ProcessingEndpoint} | {msg.EnclosedMessageTypes} | {msg.ExceptionMessage} | {msg.StackTrace} | {msg.RawMessage}";
                sb.AppendLine(psvLine);
            }

            if (CloudStorageAccount.TryParse(connString, out CloudStorageAccount cloudStorageAccount))
            {
                var byteArr = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

                using(var stream = new MemoryStream(byteArr))
                {
                    var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                    var cloudBlobContainer = cloudBlobClient.GetContainerReference(blobContainerName);
                    var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference($"{envName}.{queueName}.PeekedMessages.psv");
                    cloudBlockBlob.UploadFromStreamAsync(stream).Wait();
                    _logger.LogInformation("File Successfully uploaded");
                }
            }
            else
            {
                _logger.LogCritical("Could not connect to storage");
            }
        }
    }
}

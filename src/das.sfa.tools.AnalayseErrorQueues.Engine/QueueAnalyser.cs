using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using das.sfa.tools.AnalayseErrorQueues.services.SvcBusService;
using das.sfa.tools.AnalayseErrorQueues.services.DataSinkService;

namespace das.sfa.tools.AnalayseErrorQueues.Engine
{
    public class QueueAnalyser
    {
        IDataSink _dataSink;
        ISvcBusService _svcBusSvc;
		IConfiguration _config;
		ILogger _logger;
        
        public QueueAnalyser(IDataSink dataSink, ISvcBusService svcBusSvc, IConfiguration config, ILogger<QueueAnalyser> logger)
        {
            _dataSink = dataSink;
            _svcBusSvc = svcBusSvc;
			_config = config;
			_logger = logger;
        }

        public async Task<int> Run()
        {
            var timer = new Stopwatch();
            timer.Start();

            _logger.LogInformation("======================================================");
            _logger.LogInformation("Press ENTER key to exit after peeking all the messages.");
            _logger.LogInformation("======================================================");

            int totalMessages=0;
            var queues = _config.GetSection("ServiceBusRepoSettings:QueueNames").AsEnumerable();
            var envName = _config.GetValue<string>("ServiceBusRepoSettings:EnvName");
            foreach(var queueNameDictionary in queues)
            {
                var queueName = queueNameDictionary.Value;
                // Register the queue message handler and receive messages in a loop
                if (!String.IsNullOrEmpty(queueName))
                {
                    _logger.LogInformation($"Processing messages for queue: {queueName}");
                    var peekedMessages = await _svcBusSvc.PeekMessages(queueName);
                    totalMessages += peekedMessages.Count;
                    _dataSink.SinkMessages(envName, queueName, peekedMessages);
                    _logger.LogInformation($"Finished queue: {queueName} - processed: {peekedMessages.Count} messages");
                }
            }

            timer.Stop();
            _logger.LogInformation("");
            _logger.LogInformation($"****** Complete. Processed {totalMessages} in {timer.Elapsed.TotalSeconds} seconds");

            return 0;
        }

    }
 }

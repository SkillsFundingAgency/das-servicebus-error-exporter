using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DAS.SFA.Tools.AnalyseErrorQueues.Services.SvcBusService;
using DAS.SFA.Tools.AnalyseErrorQueues.Services.DataSinkService;
using System.Linq;

namespace DAS.SFA.Tools.AnalyseErrorQueues.Engine
{
    public class QueueAnalyser : IAnalyseQueues
    {
        private readonly IDataSink _dataSink;
        private readonly ISvcBusService _svcBusSvc;
		private readonly IConfiguration _config;
		private readonly ILogger<QueueAnalyser> _logger;
        
        public QueueAnalyser(IDataSink dataSink, ISvcBusService svcBusSvc, IConfiguration config, ILogger<QueueAnalyser> logger)
        {
            _dataSink = dataSink ?? throw new Exception("data sink is null");
            _svcBusSvc = svcBusSvc ?? throw new Exception("service is null"); 
			_config = config ?? throw new Exception("config is null");
            _logger = logger ?? throw new Exception("Logger is null");
        }

        public async Task Run()
        {
            var timer = new Stopwatch();
            timer.Start();

            int totalMessages = 0;
            var queues = _config.GetSection("ServiceBusRepoSettings:QueueNames")
                .AsEnumerable()
                .Where(dict => !string.IsNullOrEmpty(dict.Value))
            ;
            var envName = _config.GetValue<string>("ServiceBusRepoSettings:EnvName");

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Processing Queues:");
                queues.ToList().ForEach(q => _logger.LogInformation($"{q.Value}"));
                _logger.LogInformation($"envName: {envName}");
            }

            foreach (var queueNameDictionary in queues)
            {
                var queueName = queueNameDictionary.Value;
                // Register the queue message handler and receive messages in a loop
                _logger.LogInformation($"Processing messages for queue: {queueName}");
                var peekedMessages = await _svcBusSvc.PeekMessages(queueName);
                totalMessages += peekedMessages.Count;
                _dataSink.SinkMessages(envName, queueName, peekedMessages);
                _logger.LogInformation($"Finished queue: {queueName} - processed: {peekedMessages.Count} messages");
            }

            timer.Stop();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("");
                _logger.LogInformation($"****** Complete. Processed {totalMessages} in {timer.Elapsed.TotalSeconds} seconds");
            }
        }
    }
 }

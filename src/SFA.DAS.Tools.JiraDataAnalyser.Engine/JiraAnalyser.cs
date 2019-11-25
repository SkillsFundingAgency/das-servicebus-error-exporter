using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Tools.JiraDataAnalyser.Domain;
using SFA.DAS.Tools.JiraDataAnalyser.Services;
using SFA.DAS.Tools.JiraDataAnalyser.Services.DataSinkService;

namespace SFA.DAS.Tools.JiraDataAnalyser.Engine
{
    public class JiraAnalyser : IAnalyseJira
    {
        private readonly JiraSettings _options;
        private readonly ILogger<JiraAnalyser> _log;
        private readonly IJiraService _service;
        private readonly IDataSink _sink;
        private readonly IConfiguration _config;

        public JiraAnalyser(
            IOptions<JiraSettings> options,
            ILogger<JiraAnalyser> log,
            IJiraService service,
            IDataSink sink,
            IConfiguration config)
        {
            _options = options.Value ?? throw new ArgumentException("Options cannot be null", "options");
            _log = log ?? throw new ArgumentException("Log cannot be null", "log");
            _service = service ?? throw new ArgumentException("Service cannot be null", "service");
            _sink = sink ?? throw new ArgumentException("Sink cannot be null", "sink");
            _config = config ?? throw new ArgumentException("Sink cannot be null", "sink");
        }

        public async Task Run()
        {
            var sinkMessages = new List<SinkMessage>();
            _options.BoardIds.ForEach(id => _log.LogInformation($"Processing issues for board: {id}"));

            var tasks = _options.BoardIds.Select(id => _service.GetIssuesForBoard(id));
            var results = await Task.WhenAll(tasks);
            
            foreach(var issues in results)
            {
                var aggregate = _service.AggregateTransitionsToInProgress(issues);
                sinkMessages.Add(
                    new SinkMessage 
                    {
                        BoardId = issues.First().BoardId,
                        Data = aggregate.ToList().Select(i => new TransitionData(i.Key, i.Value.Count())),
                        Timestamp = DateTime.UtcNow
                    });
                 
                _log.LogInformation($"Processed board: {issues.First().BoardId}");
            }

            _log.LogInformation($"Begin Sinking messages");
            await _sink.SinkMessages("Local", sinkMessages);
            _log.LogInformation($"End Sinking messages");
        }
    }
}

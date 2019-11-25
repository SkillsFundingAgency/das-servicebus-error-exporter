using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.Tools.JiraDataAnalyser.Engine;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions
{
    public class JiraStatsExport
    {
        private readonly IAnalyseJira _analyser;

        public JiraStatsExport(IAnalyseJira analyser)
        {            
            _analyser = analyser ?? throw new Exception("Analyser is null");            
        }

        [FunctionName("JiraStatsExport")]
#if DEBUG
        public async Task Run([TimerTrigger("0 */3 * * * *")]TimerInfo timer, ILogger log)
#else
        public async Task Run([TimerTrigger("0 0 * * * *")]TimerInfo timer, ILogger log)
#endif
        {
            if (log.IsEnabled(LogLevel.Information))
            {
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                log.LogInformation(_analyser == null ? "analyser is null": $"analyser is NOT null {_analyser.GetType().ToString()}");
            }

            await _analyser.Run();
        }
    }
}

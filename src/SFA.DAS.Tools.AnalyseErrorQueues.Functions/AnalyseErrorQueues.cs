using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Tools.AnalyseErrorQueues.Engine;
using System.Threading.Tasks;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions
{
    public class AnalyseErrorQueues
    {
        private readonly IAnalyseQueues _analyser;

        public AnalyseErrorQueues(IAnalyseQueues analyser)
        {            
            _analyser = analyser ?? throw new Exception("Analyser is null");            
        }

        [FunctionName("AnalyseErrorQueues")]
#if DEBUG
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo timer, ILogger log)
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

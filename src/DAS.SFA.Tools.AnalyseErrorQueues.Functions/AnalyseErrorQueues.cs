using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using DAS.SFA.Tools.AnalyseErrorQueues.Engine;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(DAS.SFA.Tools.AnalyseErrorQueues.Functions.Startup))]

namespace DAS.SFA.Tools.AnalyseErrorQueues.Functions
{
    public class AnalyseErrorQueues
    {
        private readonly IAnalyseQueues _analyser;

        public AnalyseErrorQueues(IAnalyseQueues analyser)
        {
            _analyser = analyser;
        }

        [FunctionName("AnalyseErrorQueues")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo timer, ILogger log)
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

using Microsoft.Azure.Cosmos.Table;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions.Infrastructure
{
    public class ConfigurationItem : TableEntity
    {
        public string Data { get; set; }
    }
}
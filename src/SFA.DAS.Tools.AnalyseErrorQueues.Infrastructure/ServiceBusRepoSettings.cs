using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions.Infrastructure
{
    public class ServiceBusRepoSettings
    {
        public string ServiceBusConnectionString { get; set; }
        public string QueueSelectionRegex { get; set; }
        public int PeekMessageBatchSize { get; set; }
        public int NotifyUIBatchSize { get; set; }
        public string EnvName { get; set; }
    }
}

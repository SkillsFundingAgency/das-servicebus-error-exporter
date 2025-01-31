using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions.Infrastructure
{
    public class LADataSinkSettings
    {
        public string LogName { get; set; }
        public string TimeStampField { get; set; }
        public string workspaceId { get; set; }
        public int ChunkSize { get; set; }
        public string sharedKey { get; set; }
    }
}

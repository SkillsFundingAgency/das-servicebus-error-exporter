using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions.Infrastructure
{
    public class BlobDataSinkSettings
    {
        public string StorageConnectionString { get; set; }
        public string StorageContainerName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Functions.Infrastructure
{
    public class ApplicationConfiguration
    {
        public BlobDataSinkSettings BlobDataSinkSettings { get; set; }
        
        public ServiceBusRepoSettings ServiceBusRepoSettings { get; set; }
       
        public LADataSinkSettings LADataSinkSettings { get; set; }
    }
}

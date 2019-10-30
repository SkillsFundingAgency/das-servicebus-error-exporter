using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SFA.DAS.Tools.AnalyseErrorQueues.Domain;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Services.SvcBusService
{
    public interface ISvcBusService
    {
        Task<IList<sbMessageModel>> PeekMessages(string queueName);
    }
}

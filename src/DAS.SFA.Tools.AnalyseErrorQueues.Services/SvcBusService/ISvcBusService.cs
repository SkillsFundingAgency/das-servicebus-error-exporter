using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DAS.SFA.Tools.AnalyseErrorQueues.Domain;

namespace DAS.SFA.Tools.AnalyseErrorQueues.Services.SvcBusService
{
    public interface ISvcBusService
    {
        Task<IList<sbMessageModel>> PeekMessages(string queueName);
    }
}

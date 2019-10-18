using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using das.sfa.tools.AnalayseErrorQueues.domain;

namespace das.sfa.tools.AnalayseErrorQueues.services.SvcBusService
{
    public interface ISvcBusService
    {
        Task<IList<sbMessageModel>> PeekMessages(string queueName);
    }
}

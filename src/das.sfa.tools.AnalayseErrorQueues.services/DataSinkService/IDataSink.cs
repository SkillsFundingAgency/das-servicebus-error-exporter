using System;
using System.Collections.Generic;
using das.sfa.tools.AnalayseErrorQueues.domain;

namespace das.sfa.tools.AnalayseErrorQueues.services.DataSinkService
{
    public interface IDataSink
    {
        void SinkMessages(string envName, string queueName, IEnumerable<sbMessageModel> messages);
    }
}

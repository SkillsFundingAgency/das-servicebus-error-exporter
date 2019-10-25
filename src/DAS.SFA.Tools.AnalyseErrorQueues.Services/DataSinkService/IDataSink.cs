using System;
using System.Collections.Generic;
using DAS.SFA.Tools.AnalyseErrorQueues.Domain;

namespace DAS.SFA.Tools.AnalyseErrorQueues.Services.DataSinkService
{
    public interface IDataSink
    {
        void SinkMessages(string envName, string queueName, IEnumerable<sbMessageModel> messages);
    }
}

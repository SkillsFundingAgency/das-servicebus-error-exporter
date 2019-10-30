using System;
using System.Collections.Generic;
using SFA.DAS.Tools.AnalyseErrorQueues.Domain;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Services.DataSinkService
{
    public interface IDataSink
    {
        void SinkMessages(string envName, string queueName, IEnumerable<sbMessageModel> messages);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Tools.JiraDataAnalyser.Domain;

namespace SFA.DAS.Tools.JiraDataAnalyser.Services.DataSinkService
{
    public interface IDataSink
    {
        Task SinkMessages(string envName, IEnumerable<SinkMessage> messages);
    }
}

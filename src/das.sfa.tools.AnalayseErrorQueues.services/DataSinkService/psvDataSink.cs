using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using das.sfa.tools.AnalayseErrorQueues.domain;

namespace das.sfa.tools.AnalayseErrorQueues.services.DataSinkService
{
    public class psvDataSink : IDataSink
    {
        public void SinkMessages(string envName, string queueName, IEnumerable<sbMessageModel> messages)
        {
            var sb = new StringBuilder();
            sb.AppendLine("MessageId | TimeOfFailure | ExceptionType | OriginatingEndpoint | ProcessingEndpoint | EnclosedMessageTypes | ExceptionMessage | Stack Trace");
            foreach (var msg in messages)
            {
                var psvLine = $"{msg.MessageId} |  {msg.TimeOfFailure} | {msg.ExceptionType} | {msg.OriginatingEndpoint} | {msg.ProcessingEndpoint} | {msg.EnclosedMessageTypes} | {msg.ExceptionMessage} | {msg.StackTrace}";
                sb.AppendLine(psvLine);
            }

            File.WriteAllText($".\\{envName}.{queueName}.PeekedMessages.psv", sb.ToString());
        }
    }
}

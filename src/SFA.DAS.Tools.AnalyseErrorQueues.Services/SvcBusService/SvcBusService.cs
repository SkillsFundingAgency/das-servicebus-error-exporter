using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Tools.AnalyseErrorQueues.Domain;
using System.Text;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Services.SvcBusService
{
    public class SvcBusService : ISvcBusService
    {
        private readonly IConfiguration _config;
		private readonly ILogger _logger;

        public SvcBusService(IConfiguration config, ILogger<SvcBusService> logger)
        {
			_config = config ?? throw new Exception("config is null");
			_logger = logger ?? throw new Exception("logger is null");
        }

        public async Task<IList<sbMessageModel>> PeekMessages(string queueName)
        {
            var sbKey = _config.GetValue<string>("ServiceBusRepoSettings:ServiceBusConnectionString");
            var batchSize = _config.GetValue<int>("ServiceBusRepoSettings:PeekMessageBatchSize");
            var notifyBatchSize = _config.GetValue<int>("ServiceBusRepoSettings:NotifyUIBatchSize");
            var messageReceiver = new MessageReceiver(sbKey, queueName);

#if DEBUG
            _logger.LogDebug($"ServiceBusConnectionString: {sbKey}");
            _logger.LogDebug($"PeekMessageBatchSize: {batchSize}");
#endif

            int totalMessages = 0;
            IList<Message> peekedMessages;
            var formattedMessages = new List<sbMessageModel>();

            peekedMessages = await messageReceiver.PeekAsync(batchSize);

            _logger.LogDebug($"Peeked Message Count: {peekedMessages.Count}");

            while (peekedMessages.Count > 0)
            {
                foreach(var msg in peekedMessages)
                {
                    var messageModel = FormatMsgToLog(msg);
                    totalMessages++;
                    if (totalMessages % notifyBatchSize == 0)
                        _logger.LogDebug($"    {queueName} - processed: {totalMessages}");
               
                    formattedMessages.Add(messageModel);
                }
                peekedMessages = await messageReceiver.PeekAsync(batchSize);
            }
            await messageReceiver.CloseAsync();

            return formattedMessages;
        }

        private sbMessageModel FormatMsgToLog(Message msg)
        {
            object exceptionMessage = string.Empty;
            string exceptionMessageNoCrLf = string.Empty;
            string enclosedMessageTypeTrimmed = string.Empty;
            var messageModel = new sbMessageModel();
            if (msg.UserProperties.TryGetValue("NServiceBus.ExceptionInfo.Message", out exceptionMessage))
            {
                // this is an nServiveBusFailure.
                exceptionMessageNoCrLf = exceptionMessage.ToString().CrLfToTilde();
                enclosedMessageTypeTrimmed = msg.UserProperties.ContainsKey("NServiceBus.EnclosedMessageTypes")
                    ? msg.UserProperties["NServiceBus.EnclosedMessageTypes"].ToString().Split(',')[0]
                    : "";

                messageModel.MessageId = msg.UserProperties["NServiceBus.MessageId"].ToString();
                messageModel.TimeOfFailure = msg.UserProperties["NServiceBus.TimeOfFailure"].ToString(); 
                messageModel.ExceptionType = msg.UserProperties["NServiceBus.ExceptionInfo.ExceptionType"].ToString();
                messageModel.OriginatingEndpoint = msg.UserProperties["NServiceBus.OriginatingEndpoint"].ToString();
                messageModel.ProcessingEndpoint = msg.UserProperties["NServiceBus.ProcessingEndpoint"].ToString();
                messageModel.EnclosedMessageTypes = enclosedMessageTypeTrimmed;
                messageModel.StackTrace = msg.UserProperties["NServiceBus.ExceptionInfo.StackTrace"].ToString().CrLfToTilde();
                messageModel.ExceptionMessage = exceptionMessageNoCrLf;
            }
            else if(msg.UserProperties.TryGetValue("DeadLetterReason", out exceptionMessage))
            {
                exceptionMessageNoCrLf = exceptionMessage.ToString().CrLfToTilde();
                enclosedMessageTypeTrimmed = msg.UserProperties.ContainsKey("NServiceBus.EnclosedMessageTypes")
                    ? msg.UserProperties["NServiceBus.EnclosedMessageTypes"].ToString().Split(',')[0]
                    : "";

                messageModel.MessageId = msg.UserProperties["NServiceBus.MessageId"].ToString();
                messageModel.TimeOfFailure = msg.UserProperties["NServiceBus.TimeSent"].ToString(); 
                messageModel.ExceptionType = "Unknown";
                messageModel.OriginatingEndpoint = msg.UserProperties["NServiceBus.OriginatingEndpoint"].ToString();
                messageModel.ProcessingEndpoint = "Unknown";
                messageModel.StackTrace = string.Empty;
                messageModel.EnclosedMessageTypes = enclosedMessageTypeTrimmed;
                messageModel.ExceptionMessage = exceptionMessageNoCrLf;
            }

#if DEBUG
            // When developing I want to be able to use as simple a message as possible but still see some information in the output
            // so I will just grab the message body and output it raw
            else
            {
                _logger.LogDebug($"msg.Body: {Encoding.UTF8.GetString(msg.Body)}");
                messageModel.RawMessage = Encoding.UTF8.GetString(msg.Body);
            }
#endif
            return messageModel;
        } 
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using das.sfa.tools.AnalayseErrorQueues.domain;

namespace das.sfa.tools.AnalayseErrorQueues.services.SvcBusService
{
    public class SvcBusService : ISvcBusService
    {
		IConfiguration _config;
		ILogger _logger;

        public SvcBusService(IConfiguration config, ILogger<SvcBusService> logger)
        {
			_config = config;
			_logger = logger;
        }

        public async Task<IList<sbMessageModel>> PeekMessages(string queueName)
        {
            var messageReceiver = new MessageReceiver(_config.GetValue<string>("ServiceBusRepoSettings:ServiceBusConnectionString"), queueName);
            int totalMessages = 0;
            IList<Message> peekedMessages = null;
            var formattedMessages = new List<sbMessageModel>();
            peekedMessages = await messageReceiver.PeekAsync(_config.GetValue<int>("ServiceBusRepoSettings:PeekMessageBatchSize"));

            while(peekedMessages.Count > 0)
            {
                foreach(var msg in peekedMessages)
                {
                    var messageModel = FormatMsgToLog(msg);
                    totalMessages++;
                    if (totalMessages % _config.GetValue<int>("ServiceBusRepoSettings:NotifyUIBatchSize") == 0)
                        _logger.LogDebug($"    {queueName} - processed: {totalMessages}");
               
                    formattedMessages.Add(messageModel);
                }
                peekedMessages = await messageReceiver.PeekAsync(_config.GetValue<int>("ServiceBusRepoSettings:PeekMessageBatchSize"));
            }
            await messageReceiver.CloseAsync();

            return formattedMessages;
        }

        private sbMessageModel FormatMsgToLog(Message msg)
        {
            object exceptionMessage = "";
            string exceptionMessageNoCrLf = string.Empty;;
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
            else
            {
                msg.UserProperties.TryGetValue("DeadLetterReason", out exceptionMessage);
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
            return messageModel;
        }        
    }
}

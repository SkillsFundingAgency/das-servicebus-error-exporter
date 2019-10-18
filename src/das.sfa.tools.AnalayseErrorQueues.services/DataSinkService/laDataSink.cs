using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using das.sfa.tools.AnalayseErrorQueues.domain;

namespace das.sfa.tools.AnalayseErrorQueues.services.DataSinkService
{
    public class laDataSink : IDataSink
    {
        static string datestring = string.Empty;
		IConfiguration _config;
		ILogger _logger;

		public laDataSink(IConfiguration config, ILogger<laDataSink> logger)
		{
			_config = config;
			_logger = logger;
		}

        public void SinkMessages(string envName, string queueName, IEnumerable<sbMessageModel> messages)
        {
            // Create a hash for the API signature
			datestring = datestring == string.Empty ? DateTime.UtcNow.ToString("r") : datestring;

            // Create aggregate messages to send to azure log analytics.
            var errorsByReceivingDomain =
                from m in messages
                group m by new {m.ProcessingEndpoint, m.OriginatingEndpoint, m.EnclosedMessageTypes, m.ExceptionType} into summaryGroup
                select new
                {
                    Environment = envName,
                    Queue = queueName,
                    ProcessingEndpoint = summaryGroup.Key.ProcessingEndpoint,
                    OriginatingEndpoint = summaryGroup.Key.OriginatingEndpoint,
                    EnclosedMessageTypes = summaryGroup.Key.EnclosedMessageTypes,
                    ExceptionType = summaryGroup.Key.ExceptionType,
                    Count = summaryGroup.Count(),
                };

            // Send to log analytics in small(ish) batches.  We dont want to send 100s of messages in one go.
            var sendBatches = errorsByReceivingDomain.ChunkBy(_config.GetValue<int>("LADataSinkSettings:ChunkSize"));

            foreach(var batch in sendBatches)
            {
                var jsonList = JArray.FromObject(batch);
                var json = jsonList.ToString(Formatting.None);
                
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                string stringToHash = "POST\n" + jsonBytes.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
                string hashedString = BuildSignature(stringToHash, _config["LADataSinkSettings:sharedKey"]);
                string signature = "SharedKey " + _config["LADataSinkSettings:workspaceId"] + ":" + hashedString;

                PostData(signature, datestring, json);
            }
        }

        public static string BuildSignature(string message, string secret)
		{
			var encoding = new System.Text.ASCIIEncoding();
			byte[] keyByte = Convert.FromBase64String(secret);
			byte[] messageBytes = encoding.GetBytes(message);
			using (var hmacsha256 = new HMACSHA256(keyByte))
			{
				byte[] hash = hmacsha256.ComputeHash(messageBytes);
				return Convert.ToBase64String(hash);
			}
		}

		// Send a request to the POST API endpoint
		public void PostData(string signature, string date, string json)
		{
			try
			{
				string url = "https://" + _config["LADataSinkSettings:workspaceId"] + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

				System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
				client.DefaultRequestHeaders.Add("Accept", "application/json");
				client.DefaultRequestHeaders.Add("Log-Type", _config["LADataSinkSettings:LogName"]);
				client.DefaultRequestHeaders.Add("Authorization", signature);
				client.DefaultRequestHeaders.Add("x-ms-date", date);
				client.DefaultRequestHeaders.Add("time-generated-field", _config["LADataSinkSettings:TimeStampField"]);

				System.Net.Http.HttpContent httpContent = new StringContent(json, Encoding.UTF8);
				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				Task<System.Net.Http.HttpResponseMessage> response = client.PostAsync(new Uri(url), httpContent);

				System.Net.Http.HttpContent responseContent = response.Result.Content;
				string result = responseContent.ReadAsStringAsync().Result;
			}
			catch (Exception excep)
			{
				_logger.LogError($"Error POSTing to Log Analytics: {excep.Message}");
			}
		}        
    }
}

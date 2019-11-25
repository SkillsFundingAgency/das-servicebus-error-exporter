using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using SFA.DAS.Tools.JiraDataAnalyser.Domain;
using Microsoft.Extensions.Options;

namespace SFA.DAS.Tools.JiraDataAnalyser.Services.DataSinkService
{
    public class LaDataSink : IDataSink
    {
		private static string datestring = string.Empty;
		private readonly LogAnalyticsOptions _options;
		private readonly ILogger _logger;

		public LaDataSink(IOptions<LogAnalyticsOptions> options, ILogger<LaDataSink> logger)
		{
			_options = options.Value ?? throw new Exception("options cannot be null");
			_logger = logger ?? throw new Exception("logger cannot be null");
		}

        public async Task SinkMessages(string envName, IEnumerable<SinkMessage> messages)
        {
            // Create a hash for the API signature
#pragma warning disable S2696 // Instance members should not write to "static" fields
            datestring = datestring == string.Empty ? DateTime.UtcNow.ToString("r") : datestring;
#pragma warning restore S2696 // Instance members should not write to "static" fields

            var sharedKey = _options.SharedKey;
            var workspaceId = _options.WorkspaceId;

#if DEBUG
            _logger.LogDebug($"sharedKey: {sharedKey}");
            _logger.LogDebug($"workspaceId: {workspaceId}");
#endif

            var jsonList = JArray.FromObject(messages);
            var json = jsonList.ToString(Formatting.None);

            _logger.LogInformation($"json: {json}");

            var jsonBytes = Encoding.UTF8.GetBytes(json);
            string stringToHash = "POST\n" + jsonBytes.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
            string hashedString = BuildSignature(stringToHash, sharedKey);
            string signature = $"SharedKey {workspaceId}:{hashedString}";

            await PostData(signature, datestring, json);
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
		public async Task PostData(string signature, string date, string json)
		{
            var logName = _options.LogName;
            var timestampField = _options.TimeStampField;
            var workspaceId = _options.WorkspaceId;
            string url = string.Format(_options.Url, workspaceId);

            _logger.LogInformation($"logName: {logName}");
            _logger.LogInformation($"timestampField: {timestampField}");

#if DEBUG
            _logger.LogDebug($"workspaceId: {workspaceId}");
            _logger.LogDebug($"url: {url}");
#endif

            try
			{
				using(HttpClient client = new System.Net.Http.HttpClient())
                {
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var request = new LogAnalyticsHttpRequestMessage()
                    {
                        RequestUri = new Uri(url),
                        Method = HttpMethod.Post,
                        Content = httpContent
                    };
                    request.AddHeaders(logName, signature, date, timestampField);
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    await response.Content.ReadAsStringAsync();
                }
            }
			catch (Exception ex)
			{
				_logger.LogCritical($"Error POSTing to Log Analytics: {ex.ToString()}");
			}
		}		        
    }
}

using System;
using System.Net.Http;

namespace SFA.DAS.Tools.JiraDataAnalyser.Services
{
    public class LogAnalyticsHttpRequestMessage : HttpRequestMessage
    {

        public LogAnalyticsHttpRequestMessage()
        {
        }

        public void AddHeaders(string logName, string signature, string date, string timestampField)
        {
            this.Headers.Add("Accept", "application/json");
            this.Headers.Add("Log-Type", logName);
            this.Headers.Add("Authorization", signature);
            this.Headers.Add("x-ms-date", date);
            this.Headers.Add("time-generated-field", timestampField);
        }
    }
}
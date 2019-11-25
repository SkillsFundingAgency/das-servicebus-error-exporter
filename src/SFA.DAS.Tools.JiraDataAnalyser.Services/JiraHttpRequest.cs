using System;
using System.Net.Http;

namespace SFA.DAS.Tools.JiraDataAnalyser.Services
{
    public class JiraHttpRequestMessage : HttpRequestMessage
    {

        public JiraHttpRequestMessage(string username, string password)
        {
            AddAuthHeader(username, password);
        }

        private void AddAuthHeader(string username, string password)
        {
            var creds = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
            this.Headers.Add("Authorization", $"Basic {creds}");
        }
    }
}
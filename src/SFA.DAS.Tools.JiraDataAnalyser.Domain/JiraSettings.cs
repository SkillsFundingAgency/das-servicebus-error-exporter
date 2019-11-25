using System.Collections.Generic;

namespace SFA.DAS.Tools.JiraDataAnalyser.Domain
{
    public class JiraSettings
    {
        public string BaseUri { get; set; }
        public string IssueEndpoint { get; set; }
        public string BoardsEndpoint { get; set; }
        public List<string> BoardIds { get; set; }
        public string IssueDetailEndpoint { get; set; }
    }
}
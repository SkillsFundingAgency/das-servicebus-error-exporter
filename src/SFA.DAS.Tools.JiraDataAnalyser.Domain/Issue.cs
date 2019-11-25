namespace SFA.DAS.Tools.JiraDataAnalyser.Domain
{
    public class Issue
    {
        public string BoardId { get; set; }
        public string Key { get; set; }
        public Changelog Changelog { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SFA.DAS.Tools.JiraDataAnalyser.Domain;

namespace SFA.DAS.Tools.JiraDataAnalyser.Services
{
    public class JiraService: IJiraService
    {
        private readonly JiraSettings _options;
        private readonly JiraCredentials _credentials;
        private readonly ILogger<JiraService> _log;
        private readonly IConfiguration _config;
        private int startAt = 0;
        private int maxResults = 50;

        public JiraService(
            IOptions<JiraSettings> options,
            IOptions<JiraCredentials> credentials,
            IConfiguration config,
            ILogger<JiraService> log
        )
        {
            _options = options.Value ?? throw new ArgumentException("Options cannot be null", "options");
            _log = log ?? throw new ArgumentException("Log cannot be null", "log");
            _config = config ?? throw new ArgumentException("Config cannot be null", "config");
            _credentials = credentials.Value ?? throw new ArgumentException("Credentials cannot be null", "credentials");
        }

        public async Task<IList<Issue>> GetIssuesForBoard(string boardId, int startAt = 0)
        {
            var issuesUri = CreateIssuesUri(boardId, startAt);
            var request = CreateRequest(issuesUri);
            var issueList = new List<Issue>();

            using(HttpClient cli = new HttpClient())
            {
                try
                {
                    var resp = await cli.SendAsync(request);
                    resp.EnsureSuccessStatusCode();

                    var content = await resp.Content.ReadAsStringAsync();
                    List<Issue> list = await ParseResult(content, boardId, issueList);                    

                    _log.LogInformation("The Jira API call was successful");

                    return list.Select(l => new Issue() { BoardId = boardId, Key = l.Key, Changelog = l.Changelog }).ToList();
                }
                catch (HttpRequestException ex)
                {
                    _log.LogError($"A non success status code was returned by the JIRA Cloud api: {ex.ToString()}");
                }

                //Console.WriteLine(content);

                return new List<Issue>(0);
            }
        }

        public Dictionary<int, List<Issue>> AggregateTransitionsToInProgress(IList<Issue> issues)
        {

            issues.ToList().ForEach(issue => issue.Changelog.Histories.ForEach(history => 
                {
                    history.Items = history.Items
                        .WhereOnlyStatusItems()
                        .WhereStatusToInProgress();
                }
            ));

            var group = issues.GroupBy(
                issue => issue.Changelog.Histories
                    .Count(h => h.Items.Any())
            );
            
            return group.ToDictionary(g => g.Key, g => g.ToList());
        }

        private async Task<List<Issue>> ParseResult(string content, string boardId, List<Issue> issueList)
        {
            var jobj = JObject.Parse(content);
            var total = (int)jobj.SelectToken("total");
            var max = (int)jobj.SelectToken("maxResults");
            var issues = jobj.SelectToken("issues");

            _log.LogInformation($"total ({total}), max ({max})");

            issueList = issueList.Concat(issues.ToObject<List<Issue>>()).ToList();            

            // We need to recurse as Jira will only ever return 50 results max no matter if you ask for more or not
            if (ShouldRecurse(startAt, maxResults, total))
            {
                _log.LogInformation($"iterating with startAt: {(startAt + maxResults)}, maxResults: {maxResults}");
                var tmpList = await GetIssuesForBoard(boardId, (startAt+=maxResults));
                issueList = issueList.Concat(tmpList).ToList();
            }

            return issueList;
        }

        public async Task<IList<Changelog>> GetIssueChangelog(IList<Issue> issueModel)
        {
            throw new NotImplementedException();
        }

        private bool ShouldRecurse(int startAt, int maxResults, int total)
        {
            return (startAt + maxResults) < total;
        }

        private Uri CreateIssuesUri(string boardId, int startAt = 0)
        {
            var builder = new UriBuilder(_options.BaseUri);
            builder.Path = string.Format(_options.IssueEndpoint, boardId);
            builder.Query = $"startAt={startAt}&maxResults={maxResults}&expand=changelog";
            var issuesUri = builder.Uri;

            _log.LogDebug($"baseUri: {_options.BaseUri}");
            _log.LogDebug($"issuesUri: {issuesUri}");

#if DEBUG
    _log.LogDebug($"jira username: {_credentials.Username}");
    _log.LogDebug($"jira token: {_credentials.Token}");
#endif
            return issuesUri;
        }

        private JiraHttpRequestMessage CreateRequest(Uri issuesUri)
        {
            return new JiraHttpRequestMessage(_credentials.Username, _credentials.Token)
            {
                RequestUri = issuesUri,
                Method = HttpMethod.Get
            };
        }
    }
}

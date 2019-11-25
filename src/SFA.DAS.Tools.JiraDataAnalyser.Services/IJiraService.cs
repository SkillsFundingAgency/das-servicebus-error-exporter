using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Tools.JiraDataAnalyser.Domain;

namespace SFA.DAS.Tools.JiraDataAnalyser.Services
{
    public interface IJiraService 
    {
        Task<IList<Issue>> GetIssuesForBoard(string boardId, int startAt = 0);
        Dictionary<int, List<Issue>> AggregateTransitionsToInProgress(IList<Issue> issues);
    }
}
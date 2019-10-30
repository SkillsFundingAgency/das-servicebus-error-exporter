using System.Threading.Tasks;

namespace SFA.DAS.Tools.AnalyseErrorQueues.Engine
{
    public interface IAnalyseQueuesBase
    {
        Task Run();
    }
}
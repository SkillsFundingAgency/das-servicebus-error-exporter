using System.Threading.Tasks;

namespace DAS.SFA.Tools.AnalyseErrorQueues.Engine
{
    public interface IAnalyseQueuesBase
    {
        Task Run();
    }
}
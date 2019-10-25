using System.Threading.Tasks;

namespace DAS.SFA.Tools.AnalyseErrorQueues.Engine
{
    public interface IAnalyseQueues
    {
        Task Run();
    }
}
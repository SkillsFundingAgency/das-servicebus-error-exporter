using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Logging;
using SFA.DAS.Tools.AnalyseErrorQueues.Functions.Extensions;

internal class Program
{
    private static void Main(string[] args)
    {
        FunctionsApplication.CreateBuilder(args)
            .AddConfiguration()
            .ConfigureServices()
            .Build()
            .Run();
    }
}
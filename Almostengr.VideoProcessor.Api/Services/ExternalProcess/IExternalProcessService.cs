using System.Threading;
using System.Threading.Tasks;

namespace Almostengr.VideoProcessor.Api.Services.ExternalProcess
{
    public interface IExternalProcessService
    {
        Task RunProcessAsync(string processExecutable, string[] arguments, string workingDirectory, CancellationToken cancellationToken, int processAlarmTime = 30);
        Task RunProcessAsync(string processExecutable, string arguments, string workingDirectory, CancellationToken cancellationToken, int processAlarmTime = 30);
    }
}
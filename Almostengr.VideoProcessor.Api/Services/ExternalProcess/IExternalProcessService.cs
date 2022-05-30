using System.Threading;
using System.Threading.Tasks;

namespace Almostengr.VideoProcessor.Api.Services.ExternalProcess
{
    public interface IExternalProcessService
    {
        Task<(string stdOut, string stdErr)> RunCommandAsync(string program, string arguments, string workingDirectory, CancellationToken cancellationToken, int alarmTime = 30);
    }
}
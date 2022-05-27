using System.Threading;
using System.Threading.Tasks;

namespace Almostengr.VideoProcessor.Api.Services.ExternalProcess
{
    public interface IExternalProcessService
    {
        Task<(string stdOut, string stdErr)> ArchiveDirectoryAsync(string archiveDirectory, string archiveFileName, string workingDirectory, CancellationToken cancellationToken);
        Task RunProcessAsync(string processExecutable, string arguments, string workingDirectory, CancellationToken cancellationToken, int processAlarmTime = 30);
    }
}
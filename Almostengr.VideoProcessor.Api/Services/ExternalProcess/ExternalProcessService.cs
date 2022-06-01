using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.ExternalProcess
{
    public class ExternalProcessService : IExternalProcessService
    {
        private readonly ILogger<ExternalProcessService> _logger;

        public ExternalProcessService(ILogger<ExternalProcessService> logger)
        {
            _logger = logger;
        }

        public async Task<(string stdOut, string stdErr)> RunCommandAsync(
            string program, string arguments, string workingDirectory, CancellationToken cancellationToken, int alarmTime = 30)
        {
            _logger.LogInformation($"{program} {arguments}");

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            await process.WaitForExitAsync();

            _logger.LogInformation($"Exit code: {process.ExitCode}");

            process.Close();

            // string[] validErrors =
            int errorCount = error.Split("\n")
                .Where(x =>
                    !x.Contains("libva: /usr/lib/x86_64-linux-gnu/dri/iHD_drv_video.so init failed") &&
                    !x.Equals("")
                )
                .ToArray()
                .Count();

            if (errorCount > 0)
            {
                _logger.LogError(error);
                throw new ApplicationException("Errors occurred when running the command");
            }

            return await Task.FromResult((output, error));
        }

    }
}
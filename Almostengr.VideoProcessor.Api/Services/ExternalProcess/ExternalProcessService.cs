using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.ExternalProcess
{
    public class ExternalProcessService : IExternalProcessService
    {
        private readonly ILogger<ExternalProcessService> _logger;
        private readonly AppSettings _appSettings;

        public ExternalProcessService(ILogger<ExternalProcessService> logger, AppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        public async Task RunProcessAsync(string processExecutable, string[] arguments, string workingDirectory,
            CancellationToken cancellationToken, int processAlarmTime = 30)
        {
            await RunProcessAsync(processExecutable,
                string.Join(" ", arguments),
                workingDirectory,
                cancellationToken,
                processAlarmTime);
        }

        public async Task RunProcessAsync(string processExecutable, string arguments, string workingDirectory,
            CancellationToken cancellationToken, int processAlarmTime = 30)
        {
            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = processExecutable,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            TimeSpan startTime = DateTime.Now.TimeOfDay;

            process.Start();

            string stdError = process.StandardError.ReadToEnd();
            string stdOut = process.StandardOutput.ReadToEnd();

            while (process.HasExited == false)
            {
                TimeSpan elapsedTime = DateTime.Now.TimeOfDay - startTime;
                if (elapsedTime.TotalMinutes > processAlarmTime)
                {
                    _logger.LogWarning($"Process taking longer than expected to complete. Elapsed time: {elapsedTime}");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                process.Refresh();
            }

            process.Close();

            _logger.LogInformation(stdOut);
            _logger.LogError(stdError);
        }

    }
}
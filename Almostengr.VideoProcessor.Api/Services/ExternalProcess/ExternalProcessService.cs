using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Configuration;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.ExternalProcess
{
    public class ExternalProcessService : IExternalProcessService
    {
        private readonly ILogger<ExternalProcessService> _logger;

        public ExternalProcessService(ILogger<ExternalProcessService> logger, AppSettings appSettings)
        {
            _logger = logger;
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

            bool alarmed = false;
            TimeSpan elapsedTime = DateTime.Now.TimeOfDay;
            while (process.HasExited == false)
            {
                elapsedTime = DateTime.Now.TimeOfDay - startTime;
                if (elapsedTime.TotalMinutes > processAlarmTime && alarmed == false)
                {
                    _logger.LogWarning($"Process taking longer than expected to complete. Elapsed time: {elapsedTime}");
                    alarmed = true;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                process.Refresh();
            }

            process.Close();

            _logger.LogInformation($"Elapsed time: {elapsedTime}");
            _logger.LogInformation(stdOut);

            int validErrorCount = ExcludeSpecifiedErrors(stdError);

            if (validErrorCount > 0)
            {
                throw new ApplicationException(stdError);
            }
        }

        private int ExcludeSpecifiedErrors(string standardError)
        {
            return standardError.Split("\n").ToList()
                .Where(x => 
                    !x.Contains("libva: /usr/lib/x86_64-linux-gnu/dri/iHD_drv_video.so init failed") &&
                    !x.Equals("")
                )
                .Count();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.ExternalProcess
{
    public class ExternalProcessService : IExternalProcessService
    {
        private const string BashShell = "/bin/bash";
        private const string FfmpegBinary = "/usr/bin/ffmpeg";
        private const string FfprobeBinary = "/usr/bin/ffprobe";
        private const string TarBinary = "/bin/tar";
    
        private readonly ILogger<ExternalProcessService> _logger;

        public ExternalProcessService(ILogger<ExternalProcessService> logger)
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

            List<string> validErrors = FilterIgnorableErrors(stdError);

            if (validErrors.Count > 0)
            {
                throw new ApplicationException(validErrors.ToString());
            }
        }

        public async Task<(string stdOut, string stdErr)> ArchiveDirectoryAsync(string directoryToArchive, string archiveFileName, 
            string workingDirectory, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Archiving directory contents: {directoryToArchive}");

            return await RunCommandAsync(
                BashShell,
                $"-c \"cd \\\"{directoryToArchive}\\\" && tar -cvJf \\\"{archiveFileName}\\\" *\"",
                workingDirectory,
                cancellationToken,
                15            
            );
        }

        private async Task<(string stdOut, string stdErr)> RunCommandAsync(
            string program, string arguments, string workingDirectory, CancellationToken cancellationToken, int alarmTime = 30)
        {
            _logger.LogInformation($"Running command: {program} {arguments}");

            Process process = new Process{
                StartInfo = new ProcessStartInfo{
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

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            await process.WaitForExitAsync();

            string standardOutput = process.StandardOutput.ReadToEnd();

            string[] validErrors = process.StandardError
                .ReadToEnd()
                .Split("\n")
                .Where(x =>
                    !x.Contains("libva: /usr/lib/x86_64-linux-gnu/dri/iHD_drv_video.so init failed") &&
                    !x.Equals("")
                )
                .ToArray();

            _logger.LogInformation($"Exit code: {process.ExitCode}");

            process.Close();

            return await Task.FromResult((standardOutput, string.Concat(validErrors)));
        }

        //     await _externalProcess.RunProcessAsync(
        // ProgramPaths.BashShell,
        // $"-c \"cd \\\"{directoryToArchive}\\\" && tar -cvJf \\\"{Path.Combine(archiveDestination, archiveName)}\\\" *\"",
        // directoryToArchive,
        // cancellationToken,
        // 10);

        private List<string> FilterIgnorableErrors(string errors)
        {
            return errors.Split("\n").ToList()
                .Where(x =>
                    !x.Contains("libva: /usr/lib/x86_64-linux-gnu/dri/iHD_drv_video.so init failed") &&
                    !x.Equals("")
                )
                .ToList();
        }

    }
}
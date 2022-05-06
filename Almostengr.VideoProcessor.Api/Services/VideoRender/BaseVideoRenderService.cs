using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public abstract class BaseVideoRenderService : BaseService, IBaseVideoRenderService
    {
        private readonly ILogger<BaseVideoRenderService> _logger;
        private const int PADDING = 30;
        internal readonly string _subscribeFilter;
        internal readonly string _subscribeScrollingFilter;
        internal readonly string _upperLeft;
        internal readonly string _upperCenter;
        internal readonly string _upperRight;
        internal readonly string _centered;
        internal readonly string _lowerLeft;
        internal readonly string _lowerCenter;
        internal readonly string _lowerRight;

        public BaseVideoRenderService(ILogger<BaseVideoRenderService> logger) : base(logger)
        {
            _logger = logger;

            _upperLeft = $"x={PADDING}:y={PADDING}";
            _upperCenter = $"x=(w-tw)/2:y={PADDING}";
            _upperRight = $"x=w-tw-{PADDING}:y={PADDING}";
            _centered = $"x=(w-tw)/2:y=(h-th)/2";
            _lowerLeft = $"x={PADDING}:y=h-th-{PADDING}";
            _lowerCenter = $"x=(w-tw)/2:y=h-th-{PADDING}";
            _lowerRight = $"x=w-tw-{PADDING}:y=h-th-{PADDING}";
            
            _subscribeFilter = $"drawtext=text:'SUBSCRIBE':fontcolor={FfMpegColors.White}:fontsize={FfMpegConstants.FontSize}:{_lowerLeft}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10";
            _subscribeScrollingFilter = $"drawtext=text:'SUBSCRIBE':fontcolor={FfMpegColors.White}:fontsize={FfMpegConstants.FontSize}:x=w+(100*t):y=h-th-{PADDING}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10";
        }
        
        public abstract string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties);

        public async Task ArchiveWorkingDirectoryContentsAsync(CancellationToken cancellationToken)
        {
            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.TarBinary,
                ArgumentList = {
                    "-cvJf",
                    // TODO video title as file name with mp4 extension
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
        }

        public void CleanDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            Directory.CreateDirectory(directory);
        }

        public string[] GetVideoArchivesInDirectory(string directory)
        {
            return Directory.GetFiles(directory, $"*{FileExtension.Tar}*");
        }

        public string GetSubtitlesFilter(string workingDirectory)
        {
            string subtitleFile = Path.Combine(workingDirectory, VideoRenderFiles.SubtitlesFile);

            if (File.Exists(subtitleFile))
            {
                return $", subtitles='{subtitleFile}'";
            }

            return string.Empty;
        }

        public async Task ExtractTarFileToWorkingDirectoryAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken)
        {
            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.TarBinary,
                ArgumentList = {
                    "-xvf",
                    tarFile,
                    "-C",
                    workingDirectory
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
        }

//         public virtual async Task RenderVideoAsync(VideoPropertiesDto videoProperties)
        public virtual async Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken)
        {
            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.FfmpegBinary,
                ArgumentList = {
                    "-hide_banner",
                    "-safe",
                    "0",
                    "-loglevel",
                    FfMpegLogLevel.Error,
                    "-y",
                    "-f",
                    "concat",
                    "-i",
                    videoProperties.InputFile,
                    "-vf",
                    videoProperties.VideoFilter,
                    videoProperties.OutputFile
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
        }

        public async Task ArchiveWorkingDirectoryContentsAsync(string workingDirectory, string archiveDirectory)
        {
            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.TarBinary,
                ArgumentList = {
                    "-cvJf",
                    // TODO video title as file name with mp4 extension
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
        }

        public async Task ConvertVideoFilesToMp4Async(string directory)
        {
            var nonMp4VideoFiles = Directory.GetFiles(directory, $"*{FileExtension.Mkv}");
            // .Concat(Directory.GetFiles(directory, $"*{FileExtension.MOV}"));

            foreach (var videoFile in nonMp4VideoFiles)
            {
                string outputFilename = Path.GetFileNameWithoutExtension(videoFile) + FileExtension.Mp4;
                _logger.LogInformation($"Converting {videoFile} to {outputFilename}");

                Process process = new();
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = ProgramPaths.FfmpegBinary,
                    ArgumentList = {
                    "-hide_banner",
                    "-i",
                    Path.Combine(directory, videoFile),
                    Path.Combine(directory, outputFilename)
                },
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                Directory.Delete(Path.Combine(directory, videoFile));

                _logger.LogInformation($"Done converting {videoFile} to {outputFilename}");
            }
        }

        public void LowerCaseFileNamesInDirectory(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                File.Move(file, file.ToLower());
            }
        }

        public void CheckOrCreateFfmpegInputFile(string workingDirectory)
        {
            string inputFile = Path.Combine(workingDirectory, VideoRenderFiles.InputFile);

            if (File.Exists(inputFile) == false)
            {
                using (StreamWriter writer = new StreamWriter(inputFile))
                {
                    foreach (string file in Directory.GetFiles(workingDirectory, $"*{FileExtension.Mp4}"))
                    {
                        writer.WriteLine($"file '{file}'");
                    }
                }
            }
        }

        public void SaveVideoMetaData(VideoPropertiesDto videoProperties)
        {
            throw new NotImplementedException();
        }

        public void MoveProcessedVideoArchiveToArchive(string archiveFile, string archiveDirectory)
        {
            Directory.Move(archiveFile, archiveDirectory);
        }

        public void CreateThumbnailsFromFinalVideo(VideoPropertiesDto videoProperties)
        {
            throw new NotImplementedException();
        }
    }
}

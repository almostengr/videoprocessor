using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public class VideoRenderService : BaseService, IVideoRenderService
    {
        private readonly Random _random;
        private readonly ILogger<VideoRenderService> _logger;
        private readonly string StreetSignFilter = "fontcolor=white:fontsize=${FONTSIZE}:box=1:boxborderw=7:boxcolor=green:${LOWERCENTER}"; // TODO enter properties
        private readonly string _incomingDirectory;
        private readonly string _archiveDirectory;
        private readonly string _uploadDirectory;
        private readonly string _workingDirectory;
        private readonly string _destinationFile;
        private readonly string _majorRoadsFile;
        private readonly string _subtitlesFile;
        private readonly string _ffmpegInputFile;

        public VideoRenderService(ILogger<VideoRenderService> logger)
        {
            _logger = logger;
            _random = new Random();
            _incomingDirectory = $"{Directories.BaseDirectory}/videos/incoming";
            _archiveDirectory = $"{Directories.BaseDirectory}/videos/archive";
            _uploadDirectory = $"{Directories.BaseDirectory}/videos/upload";
            _workingDirectory = $"{Directories.BaseDirectory}/videos/working";
            _destinationFile = $"{_workingDirectory}/destination.txt";
            _majorRoadsFile = $"{_workingDirectory}/majorroads.txt";
            _subtitlesFile = $"{_workingDirectory}/subtitles.ass";
            _ffmpegInputFile = $"{_workingDirectory}/input.txt";
        }

        public string[] GetVideosFromInputDirectory()
        {
            if (Directory.Exists(_incomingDirectory) == false)
            {
                Directory.CreateDirectory(_incomingDirectory);
            }

            return Directory.GetFiles(_incomingDirectory, $"*{FileExtension.TAR}*");
        }

        public bool IsDiskSpaceAvailable()
        {
            return base.IsDiskSpaceAvailable(_incomingDirectory);
        }

        public async Task RenderChannelVideosAsync(ChannelPropertiesDto channelDto)
        {
            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.FfmpegBinary,
                ArgumentList = {
                    "-hide_banner",
                    "-i",
                    _ffmpegInputFile,
                    "-vf",
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync();
        }

        public string GetSubtitlesFilter()
        {
            if (File.Exists(_subtitlesFile))
            {
                return $", subtitles={_subtitlesFile}";
            }

            return string.Empty;
        }

        public async Task ConvertVideoFilesToMp4Async()
        {
            var nonMp4VideoFiles = Directory.GetFiles(_workingDirectory, $"*{FileExtension.MKV}*")
                .Concat(Directory.GetFiles(_workingDirectory, $"*{FileExtension.MOV}*"));

            foreach (var videoFile in nonMp4VideoFiles)
            {
                string outputFilename = Path.GetFileNameWithoutExtension(videoFile) + FileExtension.MP4;
                _logger.LogInformation($"Converting {videoFile} to {outputFilename}");

                Process process = new();
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = ProgramPaths.FfmpegBinary,
                    ArgumentList = {
                    "-hide_banner",
                    "-i",
                    Path.Combine(_workingDirectory, videoFile),
                    Path.Combine(_workingDirectory, outputFilename)
                },
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                process.Start();
                await process.WaitForExitAsync();

                Directory.Delete(Path.Combine(_workingDirectory, videoFile));

                _logger.LogInformation($"Done converting {videoFile} to {outputFilename}");
            }
        }

        private string GetBrandingText(ChannelPropertiesDto channelDto, string textColor = FfMpegColors.WHITE)
        {
            string brandingText = GetChannelBrandingText(channelDto.ChannelLabels);

            string channelBranding = $"drawtext=textfile:'{brandingText}':";
            channelBranding += $"fontcolor:{textColor}:";
            channelBranding += $"fontsize:${FfMpegConstants.FONT_SIZE}:";
            channelBranding += $""; // TODO position of upper right
            channelBranding += $"box=1:boxborderw=10:boxcolor:{FfMpegColors.BLACK}";
            return channelBranding;
        }

        private string GetChannelBrandingFilter(string channelName, int displayBrandingDuration, string channelBranding)
        {
            string ffmpegVideoFilter = string.Empty;

            switch (channelName)
            {
                case ChannelName.DashCam:
                    ffmpegVideoFilter += $"{channelBranding}:enable='between(t,0,{displayBrandingDuration})'";
                    ffmpegVideoFilter += $"{channelBranding}@{FfMpegConstants.DIMMED_BACKGROUND}:enable='gt(t,{displayBrandingDuration})'";
                    break;

                default:
                    ffmpegVideoFilter += $"{channelBranding}:enable='gt(t,0)'";
                    break;
            }

            return ffmpegVideoFilter;
        }

        public string GetDestinationFilter()
        {
            if (File.Exists($"{_workingDirectory}/destination.txt"))
            {
                return $", drawtext=textfile={_workingDirectory}/destination.txt:${StreetSignFilter}:enable='between(t,5,12)'";
            }

            return string.Empty;
        }

        public string GetMajorRoadFilter()
        {
            if (File.Exists($"{_workingDirectory}/majorroads.txt"))
            {
                return $", drawtext=textfile={_workingDirectory}/majorroads.txt:${StreetSignFilter}:enable='between(t,12,20)'";
            }

            return string.Empty;
        }

        public string GetChannelBrandingText(List<string> texts)
        {
            return texts[_random.Next(0, texts.Count - 1)];
        }

        public async Task ExtractTarFileToWorkingDirectoryAsync(string tarFile)
        {
            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.TarBinary,
                ArgumentList = {
                    "-xvf",
                    tarFile,
                    "-C",
                    _workingDirectory
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync();
        }

        public async Task RenderVideoToUploadDirectoryAsync()
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
                    FfMpegLogLevel.ERROR,
                    "-y",
                    "-f",
                    "concat",
                    "-i",
                    _ffmpegInputFile,
                    // TODO video title as file name with mp4 extension
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync();
        }

        public async Task ArchiveWorkingDirectoryContentsAsync()
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
            await process.WaitForExitAsync();
        }

        public void CleanWorkingDirectory()
        {
            if (Directory.Exists(_workingDirectory))
            {
                Directory.Delete(_workingDirectory, true);
            }

            Directory.CreateDirectory(_workingDirectory);
        }

        public string GetBrandingTextColor(string channelName, string videoTitle)
        {
            if (channelName == ChannelName.DashCam && videoTitle.ToLower().Contains("night"))
            {
                return FfMpegColors.ORANGE;
            }

            return FfMpegColors.WHITE;
        }

        public string GetFfmpegVideoFilter(string videoTitle)
        {
            string ffmpegVideoFilter = string.Empty;
            string textColor = GetBrandingTextColor(ChannelName.DashCam, videoTitle);
            int displayBrandingDuration = _random.Next(10,25);

            string brandingText = GetChannelBrandingText(channelDto.ChannelLabels);

            string channelBranding = $"drawtext=textfile:'{brandingText}':";
            channelBranding += $"fontcolor:{textColor}:";
            channelBranding += $"fontsize:${FfMpegConstants.FONT_SIZE}:";
            channelBranding += $""; // TODO position of upper right
            channelBranding += $"box=1:boxborderw=10:boxcolor:{FfMpegColors.BLACK}";
            

            switch (channelName)
            {
                case ChannelName.DashCam:
                    ffmpegVideoFilter += $"{channelBranding}:enable='between(t,0,{displayBrandingDuration})'";
                    ffmpegVideoFilter += $"{channelBranding}@{FfMpegConstants.DIMMED_BACKGROUND}:enable='gt(t,{displayBrandingDuration})'";
                    break;

                default:
                    textColor = FfMpegColors.WHITE;
                    ffmpegVideoFilter += $"{channelBranding}:enable='gt(t,0)'";
                    break;
            }

            if (File.Exists(_majorRoadsFile))
            {
                ffmpegVideoFilter += $", drawtext=textfile={_majorRoadsFile}:${StreetSignFilter}:enable='between(t,12,20)'";
            }

            if (File.Exists($"{_destinationFile}"))
            {
                ffmpegVideoFilter += $", drawtext=textfile={_destinationFile}:${StreetSignFilter}:enable='between(t,5,12)'";
            }

            return ffmpegVideoFilter;
        }

        public void CreateInputFileForFfmpeg()
        {
            if (File.Exists(_ffmpegInputFile))
            {
                return;
            }

            using (StreamWriter writer = new StreamWriter(_ffmpegInputFile))
            {
                foreach (string file in Directory.GetFiles(_workingDirectory, $"*{FileExtension.MP4}"))
                {
                    writer.WriteLine($"file '{file}'");
                }
            }
        }

        public bool DoesChannelFileExist()
        {
            throw new NotImplementedException();
        }

        public string GetDestinationFilter(string workingDirectory)
        {
            throw new NotImplementedException();
        }

        public string GetMajorRoadFilter(string workingDirectory)
        {
            throw new NotImplementedException();
        }
    }
}

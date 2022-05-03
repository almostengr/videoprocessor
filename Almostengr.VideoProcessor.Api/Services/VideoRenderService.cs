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
        private ChannelPropertiesDto _channel;

        public VideoRenderService(ILogger<VideoRenderService> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task RenderChannelVideosAsync(ChannelPropertiesDto channelDto)
        {
            _channel = channelDto;

            _logger.LogInformation($"Rendering {_channel.Name} videos");

            while (true)
            {
                if (IsDiskSpaceAvailable(_channel.InputDirectory) == false)
                {
                    _logger.LogInformation($"Not enough space to render {_channel.Name} videos");
                    break;
                }

                // get one file from the input directory
                var tarFile = Directory.GetFiles(_channel.InputDirectory, $"*{FileExtension.TAR}*").First();

                if (tarFile.Length == 0)
                {
                    _logger.LogInformation($"No tar archives found in {_channel.InputDirectory}");
                    break;
                }

                // create working directory if it does not exist

                CleanWorkingDirectory(); // clear the contents of the working directory

                string videoTitle = Path.GetFileNameWithoutExtension(tarFile);

                await ExtractTarFileToWorkingDirectoryAsync(tarFile, _channel.WorkingDirectory);

                // check the working directory for mkv or mov files;
                var nonMp4VideoFiles = Directory.GetFiles(_channel.WorkingDirectory, $"*{FileExtension.MKV}*")
                    .Concat(Directory.GetFiles(_channel.WorkingDirectory, $"*{FileExtension.MOV}*"));
                foreach (var videoFile in nonMp4VideoFiles)
                {
                    await ConvertVideoFileToMp4Async(videoFile); //  if found, convert them to mp4
                    File.Delete(videoFile); // remove originals of converted mkv and mov files
                }

                string brandingTextColor = GetBrandingTextColor(_channel.Name, videoTitle);
                string brandingText = GetBrandingText(_channel, brandingTextColor);

                int displayBrandingDuration = _random.Next(10, 25);

                string channelBranding = GetBrandingText(_channel);

                string ffmpegVideoFilter = GetChannelBrandingFilter(_channel.Name, displayBrandingDuration, channelBranding);
                ffmpegVideoFilter += GetDestinationFilter(_channel.WorkingDirectory);
                ffmpegVideoFilter += GetMajorRoadFilter(_channel.WorkingDirectory);

                ffmpegVideoFilter += GetSubtitlesFilter(); // add subtitles as filter if file present

                CreateInputFileForFfmpeg(); // create input file for ffmpeg command

                await RenderVideoToUploadDirectoryAsync(); // render video

                // copy meta data file to uploads directory, if present

                await ArchiveWorkingDirectoryContentsAsync(); // create archive with contents of working directory

                // move archive to archive directory

                // remove extracted tar file from input directory

                CleanWorkingDirectory(); // clean working directory
            }
        }

        private string GetSubtitlesFilter()
        {
            if (File.Exists(_channel.SubtitlesFile))
            {
                return $", subtitles={_channel.SubtitlesFile}";
            }

            return string.Empty;
        }

        private async Task ConvertVideoFileToMp4Async(string inputFilename)
        {
            string outputFilename = Path.GetFileNameWithoutExtension(inputFilename) + FileExtension.MP4;

            _logger.LogInformation($"Converting {inputFilename} to {outputFilename}");

            Process process = new();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = ProgramPaths.FfmpegBinary,
                ArgumentList = {
                    "-hide_banner",
                    "-i",
                    Path.Combine(_channel.WorkingDirectory, inputFilename),
                    Path.Combine(_channel.WorkingDirectory, outputFilename)
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync();
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

        private string GetDestinationFilter(string workingDirectory)
        {
            if (File.Exists(_channel.DestinationFile))
            {
                return $", drawtext=textfile={_channel.DestinationFile}:${StreetSignFilter}:enable='between(t,5,12)'";
            }

            return string.Empty;
        }

        private string GetMajorRoadFilter(string workingDirectory)
        {
            if (File.Exists(_channel.MajorRoadsFile))
            {
                return $", drawtext=textfile={_channel.MajorRoadsFile}:${StreetSignFilter}:enable='between(t,12,20)'";
            }

            return string.Empty;
        }

        private string GetChannelBrandingText(List<string> texts)
        {
            return texts[_random.Next(0, texts.Count - 1)];
        }

        private async Task ExtractTarFileToWorkingDirectoryAsync(string tarFile, string workingDirectory)
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
            await process.WaitForExitAsync();
        }

        private async Task RenderVideoToUploadDirectoryAsync()
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
                    _channel.FfmpegInputFile,
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

        private async Task ArchiveWorkingDirectoryContentsAsync()
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

        private void CleanWorkingDirectory()
        {
            if (Directory.Exists(_channel.WorkingDirectory))
            {
                Directory.Delete(_channel.WorkingDirectory, true);
            }
            
            Directory.CreateDirectory(_channel.WorkingDirectory);
        }

        private string GetBrandingTextColor(string channelName, string videoTitle)
        {
            if (channelName == ChannelName.DashCam && videoTitle.ToLower().Contains("night"))
            {
                return FfMpegColors.ORANGE;
            }

            return FfMpegColors.WHITE;
        }

        private void CreateInputFileForFfmpeg()
        {
            using (StreamWriter writer = new StreamWriter(_channel.FfmpegInputFile))
            {
                foreach (string file in Directory.GetFiles(_channel.WorkingDirectory, $"*{FileExtension.MP4}"))
                {
                    writer.WriteLine($"file '{file}'");
                }
            }
        }

    }
}

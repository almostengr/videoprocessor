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

        public VideoRenderService(ILogger<VideoRenderService> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task RenderChannelVideosAsync(ChannelPropertiesDto channel)
        {
            _logger.LogInformation($"Rendering {channel.Name} videos");

            while (true)
            {
                if (IsDiskSpaceAvailable(channel.InputDirectory) == false)
                {
                    _logger.LogInformation($"Not enough space to render {channel.Name} videos");
                    break;
                }

                // get one file from the input directory
                var tarFile = Directory.GetFiles(channel.InputDirectory, $"*{FileExtension.TAR}*").First();

                if (tarFile.Length == 0)
                {
                    _logger.LogInformation($"No tar archives found in {channel.InputDirectory}");
                    break;
                }
                
                // create working directory if it does not exist 
                
                // clear the contents of the working directory

                string videoTitle = Path.GetFileNameWithoutExtension(tarFile);

                await ExtractTarFileToWorkingDirectoryAsync(tarFile, channel.WorkingDirectory);

                // check the working directory for mkv or mov files; if found, convert them to mp4
                
                // remove originals of converted mkv and mov files

                string brandingTextColor = GetBrandingTextColor(channel.Name, videoTitle);
                string brandingText = GetBrandingText(channel, brandingTextColor);

                int displayBrandingDuration = _random.Next(10, 25);

                string channelBranding = GetBrandingText(channel);

                string ffmpegVideoFilter = GetChannelBrandingFilter(channel.Name, displayBrandingDuration, channelBranding);
                ffmpegVideoFilter += GetDestinationDetails(channel.WorkingDirectory);
                ffmpegVideoFilter += GetMajorRoadDetails(channel.WorkingDirectory);
                
                // add subtitles as filter if file present
              
                
                
                // create input file for ffmpeg command
                
                // render video
                
                // copy meta data file to uploads directory, if present
                
                // create archive with contents of working directory
                
                // move archive to archive directory
                
                // remove extracted tar file from input directory 
                
                // clean working directory

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

        private string GetDestinationDetails(string workingDirectory)
        {
            if (File.Exists(workingDirectory + "/destination.txt"))
            {
                return $", drawtext=textfile=destination.txt:${StreetSignFilter}:enable='between(t,5,12)'";
            }

            return string.Empty;
        }

        private string GetMajorRoadDetails(string workingDirectory)
        {
            if (File.Exists(workingDirectory + "/majorroads.txt"))
            {
                return $", drawtext=textfile=majorroads.txt:${StreetSignFilter}:enable='between(t,12,20)'";
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

        private string GetBrandingTextColor(string channelName, string videoTitle)
        {
            if (channelName == ChannelName.DashCam && videoTitle.ToLower().Contains("night"))
            {
                return FfMpegColors.ORANGE;
            }

            return FfMpegColors.WHITE;
        }

    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.Services.FileSystem;
using Almostengr.VideoProcessor.Constants;
using Almostengr.VideoProcessor.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Core.Services.Subtitles
{
    public class SrtSubtitleService : SubtitleService, ISrtSubtitleService
    {
        private readonly ILogger<SrtSubtitleService> _logger;
        private readonly IFileSystemService _fileSystem;
        private readonly AppSettings _appSettings;

        public SrtSubtitleService(ILogger<SrtSubtitleService> logger,
            IFileSystemService fileSystem, AppSettings appSettings) : base(logger, fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _appSettings = appSettings;
        }

        public SubtitleOutputDto CleanSubtitle(SubtitleInputDto inputDto)
        {
            _logger.LogInformation($"Cleaning subtitle file");

            string[] inputLines = inputDto.Input.Split('\n');
            int counter = 0;
            string videoString = string.Empty, blogString = string.Empty;

            foreach (var line in inputLines)
            {
                counter = counter >= 4 ? 1 : counter + 1;

                string cleanedLine = line
                    .Replace("um", string.Empty)
                    .Replace("uh", string.Empty)
                    .Replace("[music] you", "[music]")
                    .Replace("  ", " ")
                    .Replace("all right", "alright")
                    .Trim();

                videoString += cleanedLine.ToUpper() + Environment.NewLine;

                if (counter == 3)
                {
                    blogString += cleanedLine + Environment.NewLine;
                }
            }

            blogString = RemoveDuplicatesFromBlogString(blogString);
            blogString = CleanBlogString(blogString);
            blogString = ConvertToSentenceCase(blogString);

            SubtitleOutputDto outputDto = new();
            outputDto.VideoTitle = inputDto.VideoTitle.Replace(FileExtension.Srt, string.Empty);
            outputDto.VideoText = videoString;
            outputDto.BlogText = blogString;
            outputDto.BlogWords = blogString.Split(' ').Length;

            return outputDto;
        }

        public void SaveSubtitleFile(SubtitleOutputDto subtitleDto, string archiveDirectory)
        {
            base.SaveFileContents(
                Path.Combine(archiveDirectory, $"{subtitleDto.VideoTitle}{FileExtension.Srt}"),
                subtitleDto.VideoText);

            base.SaveFileContents(
                Path.Combine(archiveDirectory, $"{subtitleDto.VideoTitle}{FileExtension.Md}"),
                subtitleDto.BlogText);
        }

        public void ArchiveSubtitleFile(string transcriptFilePath, string archiveDirectory)
        {
            _fileSystem.MoveFile(transcriptFilePath, archiveDirectory);
        }

        public string[] GetIncomingSubtitles(string directory)
        {
            return _fileSystem.GetFilesInDirectory(directory)
                .Where(x => x.EndsWith(FileExtension.Srt))
                .ToArray();
        }

        public override bool IsValidFile(SubtitleInputDto inputDto)
        {
            if (string.IsNullOrEmpty(inputDto.Input))
            {
                _logger.LogError("Input is empty");
                return false;
            }

            string[] inputLines = inputDto.Input.Split('\n');
            return (inputLines[0].StartsWith("1") == true && inputLines[1].StartsWith("00:") == true);
        }

        public async Task WorkerIdleAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerIdleInterval), cancellationToken);
        }
    }
}
using System;
using System.IO;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Services.TextFile;
using Almostengr.VideoProcessor.Constants;
using Almostengr.VideoProcessor.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.Subtitles
{
    public class SrtSubtitleService : BaseSubtitleService, ISubtitleService
    {
        private readonly ILogger<SrtSubtitleService> _logger;
        private readonly ITextFileService _textFileService;

        public SrtSubtitleService(ILogger<SrtSubtitleService> logger, ITextFileService textFileService,
            AppSettings appSettings) : base(logger, appSettings)
        {
            _logger = logger;
            _textFileService = textFileService;
        }

        public SubtitleOutputDto CleanTranscript(SubtitleInputDto inputDto)
        {
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

            blogString = RemoveDupesFromBlogString(blogString);
            blogString = CleanBlogString(blogString);
            blogString = ProcessSentenceCase(blogString);

            SubtitleOutputDto outputDto = new();
            outputDto.VideoTitle = inputDto.VideoTitle.Replace(FileExtension.Srt, string.Empty);
            outputDto.VideoText = videoString;
            outputDto.BlogText = blogString;
            outputDto.BlogWords = blogString.Split(' ').Length;

            _logger.LogInformation("Transcript processed successfully");

            return outputDto;
        }

        public void SaveTranscript(SubtitleOutputDto transcriptDto, string archiveDirectory)
        {
            _textFileService.SaveFileContents(
                Path.Combine(archiveDirectory, $"{transcriptDto.VideoTitle}.{FileExtension.Srt}"),
                transcriptDto.VideoText);

            _textFileService.SaveFileContents(
                Path.Combine(archiveDirectory, $"{transcriptDto.VideoTitle}.{FileExtension.Md}"),
                transcriptDto.BlogText);
        }

        public void ArchiveTranscript(string transcriptFilePath, string archiveDirectory)
        {
            base.MoveFile(transcriptFilePath, archiveDirectory);
        }

        public string[] GetIncomingTranscripts(string directory)
        {
            return base.GetDirectoryContents(directory, $"*{FileExtension.Srt}");
        }

        public override bool IsValidTranscript(SubtitleInputDto inputDto)
        {
            if (string.IsNullOrEmpty(inputDto.Input))
            {
                _logger.LogError("Input is empty");
                return false;
            }

            string[] inputLines = inputDto.Input.Split('\n');
            return (inputLines[0].StartsWith("1") == true && inputLines[1].StartsWith("00:") == true);
        }

    }
}

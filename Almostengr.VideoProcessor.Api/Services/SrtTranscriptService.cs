using System.IO;
using Almostengr.VideoProcessor.Constants;
using Almostengr.VideoProcessor.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Services
{
    public class SrtTranscriptService : BaseTranscriptService, ITranscriptService
    {
        private readonly ILogger<SrtTranscriptService> _logger;
        private readonly ITextFileService _textFileService;

        public SrtTranscriptService(ILogger<SrtTranscriptService> logger, ITextFileService textFileService)
        {
            _logger = logger;
            _textFileService = textFileService;
        }

        public TranscriptOutputDto CleanTranscript(TranscriptInputDto inputDto)
        {
            string[] inputLines = inputDto.Input.Split('\n');
            int counter = 0;
            string videoString = string.Empty;
            string blogString = string.Empty;

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

                videoString += cleanedLine.ToUpper() + Formatting.NewLine;

                if (counter == 3)
                {
                    blogString += cleanedLine + Formatting.NewLine;
                }
            }

            blogString = RemoveDupesFromBlogString(blogString);
            blogString = CleanBlogString(blogString);
            blogString = ProcessSentenceCase(blogString);

            TranscriptOutputDto outputDto = new();
            outputDto.VideoTitle = inputDto.VideoTitle.Replace(FileExtension.Srt, string.Empty);
            outputDto.VideoText = videoString;
            outputDto.BlogText = blogString;
            outputDto.BlogWords = blogString.Split(' ').Length;

            _logger.LogInformation("Transcript processed successfully");

            return outputDto;
        }
        
        public void SaveTranscript(TranscriptOutputDto transcriptDto)
        {
            _textFileService.SaveFileContents(
                $"{Transcript.OutputDirectory}/{transcriptDto.VideoTitle}.srt",
                outputDto.VideoText);

            _textFileService.SaveFileContents(
                $"{Transcript.OutputDirectory}/{transcriptDto.VideoTitle}.md",
                outputDto.BlogText);
        }
        
        public void ArchiveTranscript(string transcriptFilename)
        {
            Directory.Move($"{Transcript.InputDirectory}/{transcriptFilename}", "{Transcript.OutputDirectory}/{transcriptFilename}");
        }

        public string[] GetTranscriptList(string srt)
        {
            if (Directory.Exists(Transcript.InputDirectory) == false)
            {
                Directory.CreateDirectory(Transcript.InputDirectory);
            }

            return Directory.GetFiles(Transcript.InputDirectory, $"*{FileExtension.Srt}");
        }

        public override bool IsValidTranscript(TranscriptInputDto inputDto)
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

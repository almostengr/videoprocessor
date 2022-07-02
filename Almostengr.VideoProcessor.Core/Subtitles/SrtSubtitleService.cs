using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Core.Subtitles
{
    public class SrtSubtitleService : SubtitleService, ISrtSubtitleService
    {
        private readonly ILogger<SrtSubtitleService> _logger;
        private readonly IBaseService _BaseService;
        private readonly AppSettings _appSettings;

        private readonly string _incomingDirectory;
        private readonly string _uploadDirectory;

        public SrtSubtitleService(ILogger<SrtSubtitleService> logger,
            IBaseService BaseService, AppSettings appSettings) : base(logger)
        {
            _logger = logger;
            _BaseService = BaseService;
            _appSettings = appSettings;

            _incomingDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "incoming");
            _uploadDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "upload");
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
            _BaseService.MoveFile(transcriptFilePath, archiveDirectory);
        }

        public string[] GetIncomingSubtitles(string directory)
        {
            return _BaseService.GetFilesInDirectory(directory)
                .Where(x => x.EndsWith(FileExtension.Srt))
                .ToArray();
        }

        public async Task WorkerIdleAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerIdleInterval), cancellationToken);
        }

        private string GetRandomSubtitleFile(string directory)
        {
            Random random = new();
            return GetIncomingSubtitles(directory)
                .Where(x => x.StartsWith(".") == false)
                .OrderBy(x => random.Next()).Take(1).FirstOrDefault();
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string subtitleFile = GetRandomSubtitleFile(_incomingDirectory);
                bool isDiskSpaceAvailable = 
                    _BaseService.IsDiskSpaceAvailable(_incomingDirectory, _appSettings.DiskSpaceThreshold);

                if (string.IsNullOrEmpty(subtitleFile) || isDiskSpaceAvailable == false)
                {
                    await WorkerIdleAsync(stoppingToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation($"Processing {subtitleFile}");

                    await _BaseService.ConfirmFileTransferCompleteAsync(subtitleFile);

                    string fileContent = GetFileContents(subtitleFile);

                    SubtitleInputDto subtitleInputDto =
                        new SubtitleInputDto(fileContent, Path.GetFileName(subtitleFile));

                    if (subtitleInputDto.IsValidSrtSubtitleFile() == false)
                    {
                        _logger.LogError($"{subtitleFile} is not in a valid format");
                        continue;
                    }

                    SubtitleOutputDto transcriptOutput = CleanSubtitle(subtitleInputDto);

                    SaveSubtitleFile(transcriptOutput, _uploadDirectory);
                    ArchiveSubtitleFile(subtitleFile, _uploadDirectory);

                    _logger.LogInformation($"Finished processing {subtitleFile}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.InnerException, ex.Message);
                }
            } // end while
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _BaseService.CreateDirectory(_incomingDirectory);
            _BaseService.CreateDirectory(_uploadDirectory);
            await Task.CompletedTask;
        }
    }
}

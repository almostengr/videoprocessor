using Almostengr.VideoProcessor.DataTransferObjects;
using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.Services.Subtitles;
using Almostengr.VideoProcessor.Core.Services.FileSystem;

namespace Almostengr.VideoProcessor.Worker
{
    public class HandyTechSubtitleWorker : BackgroundService
    {
        private readonly ISrtSubtitleService _subtitleService;
        private readonly AppSettings _appSettings;
        private readonly IFileSystemService _fileSystemService;
        private readonly ILogger<HandyTechSubtitleWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _uploadDirectory;

        public HandyTechSubtitleWorker(ILogger<HandyTechSubtitleWorker> logger, IServiceScopeFactory factory)
        {
            _subtitleService = factory.CreateScope().ServiceProvider.GetRequiredService<ISrtSubtitleService>();
            _appSettings = factory.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
            _fileSystemService = factory.CreateScope().ServiceProvider.GetRequiredService<IFileSystemService>();
            _logger = logger;
            _incomingDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "incoming");
            _uploadDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "upload");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _fileSystemService.CreateDirectory(_incomingDirectory);
            _fileSystemService.CreateDirectory(_uploadDirectory);
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                string subtitleFile = _subtitleService.GetIncomingSubtitles(_incomingDirectory)
                    .Where(x => x.StartsWith(".") == false)
                    .OrderBy(x => random.Next()).Take(1).FirstOrDefault();
                bool isDiskSpaceAvailable = _fileSystemService.IsDiskSpaceAvailable(_incomingDirectory, _appSettings.DiskSpaceThreshold);

                if (string.IsNullOrEmpty(subtitleFile) || isDiskSpaceAvailable == false)
                {
                    await _subtitleService.WorkerIdleAsync(stoppingToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation($"Processing {subtitleFile}");

                    await _fileSystemService.ConfirmFileTransferCompleteAsync(subtitleFile);

                    string fileContent = _subtitleService.GetFileContents(subtitleFile);

                    SubtitleInputDto subtitleInputDto = new SubtitleInputDto
                    {
                        Input = fileContent,
                        VideoTitle = Path.GetFileName(subtitleFile)
                    };

                    if (_subtitleService.IsValidFile(subtitleInputDto) == false)
                    {
                        _logger.LogError($"{subtitleFile} is not in a valid format");
                        continue;
                    }

                    SubtitleOutputDto transcriptOutput = _subtitleService.CleanSubtitle(subtitleInputDto);

                    _subtitleService.SaveSubtitleFile(transcriptOutput, _uploadDirectory);
                    _subtitleService.ArchiveSubtitleFile(subtitleFile, _uploadDirectory);

                    _logger.LogInformation($"Finished processing {subtitleFile}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.InnerException, ex.Message);
                }
            } // end while
        }

    }
}

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.DataTransferObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Services.Subtitles;
using Almostengr.VideoProcessor.Api.Services.TextFile;
using System;
using System.Linq;
using Almostengr.VideoProcessor.Api.Services.FileSystem;

namespace Almostengr.VideoProcessor.Workers
{
    public class RhtServicesSubtitleWorker : BackgroundService
    {
        private readonly ISrtSubtitleService _transcriptService;
        private readonly ITextFileService _textFileService;
        private readonly AppSettings _appSettings;
        private readonly IFileSystemService _fileSystemService;
        private readonly ILogger<RhtServicesSubtitleWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _uploadDirectory;

        public RhtServicesSubtitleWorker(ILogger<RhtServicesSubtitleWorker> logger, IServiceScopeFactory factory)
        {
            _transcriptService = factory.CreateScope().ServiceProvider.GetRequiredService<ISrtSubtitleService>();
            _textFileService = factory.CreateScope().ServiceProvider.GetRequiredService<ITextFileService>();
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
                string transcriptFile = _transcriptService.GetIncomingTranscripts(_incomingDirectory)
                    .Where(x => x.StartsWith(".") == false)
                    .OrderBy(x => random.Next()).Take(1).FirstOrDefault();
                bool isDiskSpaceAvailable = _fileSystemService.IsDiskSpaceAvailable(_incomingDirectory, _appSettings.DiskSpaceThreshold);

                if (string.IsNullOrEmpty(transcriptFile) || isDiskSpaceAvailable == false)
                {
                    await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerServiceInterval), stoppingToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation($"Processing {transcriptFile}");

                    await _fileSystemService.ConfirmFileTransferCompleteAsync(transcriptFile);

                    string fileContent = _textFileService.GetFileContents(transcriptFile);

                    SubtitleInputDto transcriptInputDto = new SubtitleInputDto
                    {
                        Input = fileContent,
                        VideoTitle = Path.GetFileName(transcriptFile)
                    };

                    if (_transcriptService.IsValidFile(transcriptInputDto) == false)
                    {
                        _logger.LogError($"{transcriptFile} is not in a valid format");
                        continue;
                    }

                    SubtitleOutputDto transcriptOutput = _transcriptService.CleanTranscript(transcriptInputDto);

                    _transcriptService.SaveTranscript(transcriptOutput, _uploadDirectory);
                    _transcriptService.ArchiveTranscript(transcriptFile, _uploadDirectory);

                    _logger.LogInformation($"Finished processing {transcriptFile}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.InnerException, ex.Message);
                }
            } // end while
        }

    }
}

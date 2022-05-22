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

namespace Almostengr.VideoProcessor.Workers
{
    public class SrtSubtitleWorker : BackgroundService
    {
        private readonly ISubtitleService _transcriptService;
        private readonly ITextFileService _textFileService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<SrtSubtitleWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _outgoingDirectory;

        public SrtSubtitleWorker(ILogger<SrtSubtitleWorker> logger, IServiceScopeFactory factory)
        {
            _transcriptService = factory.CreateScope().ServiceProvider.GetRequiredService<ISubtitleService>();
            _textFileService = factory.CreateScope().ServiceProvider.GetRequiredService<ITextFileService>();
            _appSettings = factory.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
            _logger = logger;
            _incomingDirectory = Path.Combine(_appSettings.Directories.TranscriptBaseDirectory, "incoming");
            _outgoingDirectory = Path.Combine(_appSettings.Directories.TranscriptBaseDirectory, "outgoing");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _transcriptService.CreateDirectory(_incomingDirectory);
            _transcriptService.CreateDirectory(_outgoingDirectory);
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                string transcriptFile = _transcriptService.GetIncomingTranscripts(_incomingDirectory)
                    .OrderBy(x => random.Next()).Take(1).FirstOrDefault();
                bool isDiskSpaceAvailable = _transcriptService.IsDiskSpaceAvailable(_incomingDirectory);

                if (string.IsNullOrEmpty(transcriptFile) || isDiskSpaceAvailable == false)
                {
                    await Task.Delay(TimeSpan.FromMinutes(_appSettings.WorkerServiceInterval), stoppingToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation($"Processing {transcriptFile}");

                    await _textFileService.ConfirmFileTransferCompleteAsync(transcriptFile);

                    string fileContent = _textFileService.GetFileContents(transcriptFile);

                    SubtitleInputDto transcriptInputDto = new SubtitleInputDto
                    {
                        Input = fileContent,
                        VideoTitle = Path.GetFileName(transcriptFile)
                    };

                    if (_transcriptService.IsValidTranscript(transcriptInputDto) == false)
                    {
                        _logger.LogError($"{transcriptFile} is not in a valid format");
                        continue;
                    }

                    SubtitleOutputDto transcriptOutput = _transcriptService.CleanTranscript(transcriptInputDto);

                    _transcriptService.SaveTranscript(transcriptOutput, _outgoingDirectory);
                    _transcriptService.ArchiveTranscript(transcriptFile, _outgoingDirectory);

                    _logger.LogInformation($"Finished processing {transcriptFile}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            } // end while
        }

    }
}

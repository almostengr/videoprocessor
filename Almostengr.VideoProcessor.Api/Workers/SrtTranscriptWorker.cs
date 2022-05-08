using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Almostengr.VideoProcessor.Api.Common;

namespace Almostengr.VideoProcessor.Workers
{
    public class SrtTranscriptWorker : BackgroundService
    {
        private readonly ITranscriptService _transcriptService;
        private readonly ITextFileService _textFileService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<SrtTranscriptWorker> _logger;
        private readonly string _incomingDirectory;
        private readonly string _outgoingDirectory;

        public SrtTranscriptWorker(ILogger<SrtTranscriptWorker> logger, IServiceScopeFactory factory)
        {
            _transcriptService = factory.CreateScope().ServiceProvider.GetRequiredService<ITranscriptService>();
            _textFileService = factory.CreateScope().ServiceProvider.GetRequiredService<ITextFileService>();
            _appSettings = factory.CreateScope().ServiceProvider.GetRequiredService<AppSettings>();
            _logger = logger;
            _incomingDirectory = Path.Combine(_appSettings.Directories.BaseDirectory, "transcript/incoming");
            _outgoingDirectory = Path.Combine(_appSettings.Directories.BaseDirectory, "transcript/outgoing");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _transcriptService.CreateDirectory(_incomingDirectory);
            _transcriptService.CreateDirectory(_outgoingDirectory);
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var srtTranscripts = _transcriptService.GetIncomingTranscripts();
                
                foreach (var transcriptFile in srtTranscripts)
                {
                    _logger.LogInformation($"Processing {transcriptFile}");
                    string fileContent = _textFileService.GetFileContents(transcriptFile); // get the file contents

                    TranscriptInputDto transcriptInputDto = new TranscriptInputDto
                    {
                        Input = fileContent,
                        VideoTitle = Path.GetFileName(transcriptFile)
                    };

                    if (_transcriptService.IsValidTranscript(transcriptInputDto) == false)
                    {
                        _logger.LogError($"{transcriptFile} is not in a valid format");
                        continue;
                    }
                    
                    TranscriptOutputDto transcriptOutput = _transcriptService.CleanTranscript(transcriptInputDto);
                    
                    _transcriptService.SaveTranscript(transcriptOutput); // save the transcript (video, blog, original) to the output directory
                    
                    _transcriptService.ArchiveTranscript(transcriptFile); // move the original file from the incoming to the output directory
                                      
                    _logger.LogInformation($"Finished processing {transcriptFile}");
                }

                if (srtTranscripts.Length == 0)
                {
                    _logger.LogInformation("No new transcripts found");
                    await Task.Delay(_appSettings.WorkerServiceInterval, stoppingToken);
                }
            }
        }

    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class SrtTranscriptWorker : BackgroundService
    {
        private readonly ITranscriptService _transcriptService;
        private readonly ITextFileService _textFileService;
        private readonly ILogger<SrtTranscriptWorker> _logger;

        public SrtTranscriptWorker(ILogger<SrtTranscriptWorker> logger, IServiceScopeFactory factory)
        {
            _transcriptService = factory.CreateScope().ServiceProvider.GetRequiredService<ITranscriptService>();
            _textFileService = factory.CreateScope().ServiceProvider.GetRequiredService<ITextFileService>();
            _logger = logger;
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
                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }
            }
        }

    }
}

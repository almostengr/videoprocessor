using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Constants;
using Almostengr.VideoProcessor.DataTransferObjects;
using Almostengr.VideoProcessor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class TranscriptWorker : BackgroundService
    {
        private readonly ITranscriptService _transcriptService;
        private readonly ITextFileService _textFileService;
        private readonly ILogger<TranscriptWorker> _logger;

        public TranscriptWorker(ITranscriptService transcriptService, ITextFileService textFileService,
            ILogger<TranscriptWorker> logger)
        {
            _transcriptService = transcriptService;
            _textFileService = textFileService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var srtTranscripts = _transcriptService.GetTranscriptList(FileExtension.SRT);

                foreach (var srtTranscript in srtTranscripts)
                {
                    _logger.LogInformation($"TranscriptWorker: Processing {srtTranscript}");
                    var fileContent = _textFileService.GetFileContents(srtTranscript);

                    var transcriptOutput = _transcriptService.CleanTranscript(new TranscriptInputDto
                    {
                        Input = fileContent,
                        // VideoTitle = srtTranscript.Replace(TranscriptExt.Srt, string.Empty)
                        VideoTitle = Path.GetFileName(srtTranscript)
                    });

                    if (transcriptOutput != null)
                    {
                        _textFileService.DeleteFile(srtTranscript);
                    }
                    
                    _logger.LogInformation($"TranscriptWorker: Finished processing {srtTranscript}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
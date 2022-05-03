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

        // public TranscriptWorker(ITranscriptService transcriptService, ITextFileService textFileService,            ILogger<TranscriptWorker> logger)
        public TranscriptWorker(ITranscriptService transcriptService, IServiceScopeFactory factory)
        {
            _transcriptService = factory.CreateScope().ServiceProvider.GetRequiredService<ITranscriptService>();
            _textFileService = factory.CreateScope().ServiceProvider.GetRequiredService<ITextFileService>();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime startTime = DateTime.Now;
                
                var srtTranscripts = _transcriptService.GetTranscriptList(FileExtension.SRT);
                
                foreach (var transcriptFile in srtTranscripts)
                {
                    _logger.LogInformation($"TranscriptWorker: Processing {srtTranscript}");
                    var fileContent = _transcriptService.GetFileContents(transcriptFile); // get the file contents 
                    
                    // check the format
                    
                    // clean the transcript
                    var transcriptOutput = _transcriptService.CleanTranscript(new TranscriptInputDto
                    {
                        Input = fileContent,
                        VideoTitle = Path.GetFileName(transcriptFile)
                    });
                    
                    _transcriptService.SaveTranscript(transcriptOutput); // save the transcript (video, blog, original) to the output directory
                    
                    _transcriptService.ArchiveTranscript(transcriptFile); // move the original file from the incoming to the output directory
                                      
                    _logger.LogInformation($"TranscriptWorker: Finished processing {srtTranscript}");
                }

//                 foreach (var srtTranscript in srtTranscripts)
//                 {
//                     _logger.LogInformation($"TranscriptWorker: Processing {srtTranscript}");
//                     var fileContent = _textFileService.GetFileContents(srtTranscript);

//                     var transcriptOutput = _transcriptService.CleanTranscript(new TranscriptInputDto
//                     {
//                         Input = fileContent,
//                         // VideoTitle = srtTranscript.Replace(TranscriptExt.Srt, string.Empty)
//                         VideoTitle = Path.GetFileName(srtTranscript)
//                     });

//                     if (transcriptOutput != null)
//                     {
//                         _textFileService.DeleteFile(srtTranscript);
//                     }
                    
//                     _logger.LogInformation($"TranscriptWorker: Finished processing {srtTranscript}");
//                 }

                TimeSpan elapsedTime = DateTime.Now - startTime;

                if (elapsedTime.TotalMinutes < 1)
                {
                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }
            }
        }
    }
}

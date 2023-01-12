// using Almostengr.VideoProcessor.Core.Common;
// using Almostengr.VideoProcessor.Core.Subtitles.HandymanSubtitle;

// namespace Almostengr.VideoProcessor.Worker.Workers;

// internal sealed class HandymanSubtitleWorker : BaseWorker
// {
//     private readonly IHandymanSubtitleService _service;
//     private readonly AppSettings _appSettings;

//     public HandymanSubtitleWorker(IHandymanSubtitleService service,
//         AppSettings appSettings) : base(appSettings)
//     {
//         _service = service;
//         _appSettings = appSettings;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             bool doDelay = await _service.ExecuteAsync(stoppingToken);
            
//             if (doDelay)
//             {
//                 await Task.Delay(_appSettings.WorkerDelay, stoppingToken);
//             }
//         }
//     }
// }

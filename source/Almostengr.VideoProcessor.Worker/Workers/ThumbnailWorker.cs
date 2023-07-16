// using Almostengr.VideoProcessor.Core.Common;
// using Almostengr.VideoProcessor.Core.Handyman;
// using Almostengr.VideoProcessor.Core.TechTalk;

// namespace Almostengr.VideoProcessor.Worker.Workers;

// internal sealed class ThumbnailWorker : BaseWorker
// {
//     private readonly IHandymanVideoService _handymanVideoService;
//     private readonly ITechTalkVideoService _techTalkVideoService;
//     internal readonly new TimeSpan _delayLoopTime;

//     public ThumbnailWorker(AppSettings appSettings,
//         IHandymanVideoService handymanVideoService, ITechTalkVideoService techtalkvideoservice) : base(appSettings)
//     {
//         _handymanVideoService = handymanVideoService;
//         _techTalkVideoService = techtalkvideoservice;
//         _delayLoopTime = new TimeSpan(0, 0, 1);
//     }

//     protected override async Task ExecuteAsync(CancellationToken cancellationToken)
//     {
//         DateTime previousTime = DateTime.Now;

//         while (!cancellationToken.IsCancellationRequested)
//         {
//             _handymanVideoService.CreateThumbnails();
//             _techTalkVideoService.CreateThumbnails();
//             previousTime = await DelayWhenLoopingQuickly(_delayLoopTime, cancellationToken, previousTime);
//         }
//     }
// }
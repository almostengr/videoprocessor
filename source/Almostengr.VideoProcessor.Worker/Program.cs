using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.FileSystem;
using Almostengr.VideoProcessor.Infrastructure.Logging;
using Almostengr.VideoProcessor.Infrastructure.Processes;
using Almostengr.VideoProcessor.Core.Handyman;
using Almostengr.VideoProcessor.Worker.Workers;
using Almostengr.VideoProcessor.Infrastructure.Common;
using Almostengr.VideoProcessor.Core.Toastmasters;
using Almostengr.VideoProcessor.Core.TechTalk;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.DashCam;
using Almostengr.VideoProcessor.Infrastructure.Web;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(new AppSettings());

        services.AddSingleton<IDashCamVideoService, DashCamService>();
        services.AddSingleton<IFfmpegService, FfmpegService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IFileCompressionService, XzService>();
        services.AddSingleton<IGzFileCompressionService, GzipService>();
        services.AddSingleton<IXzFileCompressionService, XzService>();
        services.AddSingleton<IHandymanVideoService, HandymanService>();
        services.AddSingleton<IMusicService, MusicService>();
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<ITarballService, TarballService>();
        services.AddSingleton<ITechTalkVideoService, TechTalkService>();
        services.AddSingleton<IToastmastersVideoService, ToastmastersService>();
        services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));
        services.AddSingleton<IThumbnailService, ThumbnailService>();

        services.AddSingleton<ISrtSubtitleFileService, SrtSubtitleFileService>();
        services.AddSingleton<IAssSubtitleFileService, AssSubtitleFileService>();

        // services.AddSingleton<SubtitleWorker>();
        services.AddSingleton<VideoWorker>();
    })
    .UseSystemd()
    .Build();

await host.RunAsync();

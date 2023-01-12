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

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(new AppSettings());

        services.AddSingleton<IDashCamVideoService, DashCamVideoService>();
        services.AddSingleton<IFfmpegService, FfmpegService>();
        services.AddSingleton<IFileSystemService, FileSystem>();
        services.AddSingleton<IGzipService, GzipService>();
        services.AddSingleton<IHandymanVideoService, HandymanVideoService>();
        services.AddSingleton<IMusicService, MusicService>();
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<ITarballService, TarballService>();
        services.AddSingleton<ITechTalkVideoService, TechTalkVideoService>();
        services.AddSingleton<IToastmastersVideoService, ToastmastersVideoService>();
        services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));

        services.AddHostedService<DashCamVideoWorker>();
        services.AddHostedService<HandymanVideoWorker>();
        services.AddHostedService<TechTalkVideoWorker>();
        services.AddHostedService<ToastmastersVideoWorker>();
    })
    .UseSystemd()
    .Build();

await host.RunAsync();

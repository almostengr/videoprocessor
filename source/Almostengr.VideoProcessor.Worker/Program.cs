using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Worker.Workers;
using Almostengr.VideoProcessor.Infrastructure.FileSystem;
using Almostengr.VideoProcessor.Infrastructure.Logging;
using Almostengr.VideoProcessor.Infrastructure.Processes;
using Almostengr.VideoProcessor.Domain.Videos.DashCam;
using Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;
using Almostengr.VideoProcessor.Domain.Videos.Handyman;
using Almostengr.VideoProcessor.Domain.Toastmasters;
using Almostengr.VideoProcessor.Domain.Videos.ChristmasLightShow;
using Almostengr.VideoProcessor.Domain.Videos.Technology;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(new AppSettings());

        services.AddSingleton<IChristmasLightVideoService, ChristmasLightVideoService>();
        services.AddSingleton<IDashCamVideoService, DashCamVideoService>();
        services.AddSingleton<IFfmpeg, Ffmpeg>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IHandymanSubtitleService, HandymanSubtitleService>();
        services.AddSingleton<IHandymanVideoService, HandymanVideoService>();
        services.AddSingleton<IMusicService, MusicService>();
        services.AddSingleton<ITarball, Tarball>();
        services.AddSingleton<ITechnologyVideoService, TechnologyVideoService>();
        services.AddSingleton<IToastmastersVideoService, ToastmastersVideoService>();
        services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));

        // services.AddHostedService<ChristmasLightVideoWorker>();
        // services.AddHostedService<DashCamVideoWorker>();
        // services.AddHostedService<HandymanSubtitleWorker>();
        // services.AddHostedService<HandymanVideoWorker>();
        services.AddHostedService<TechnologyVideoWorker>();
        // services.AddHostedService<ToastmastersVideoWorker>();
    })
    .UseSystemd()
    .Build();

await host.RunAsync();

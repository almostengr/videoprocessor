using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Worker.Workers;
using Almostengr.VideoProcessor.Infrastructure.FileSystem;
using Almostengr.VideoProcessor.Infrastructure.Logging;
using Almostengr.VideoProcessor.Infrastructure.Processes;
using Almostengr.VideoProcessor.Domain.Videos.DashCamVideo;
using Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;
using Almostengr.VideoProcessor.Domain.Videos.HandymanVideo;
using Almostengr.VideoProcessor.Domain.ToastmastersVideo;

// string environment = string.Empty;

// #if RELEASE
//     environment = AppEnvironment.Prod;
// #else
//     environment = AppEnvironment.Devl;
// #endif

// IConfiguration configuration = new ConfigurationBuilder()
// .AddJsonFile(
//     (environment == AppEnvironment.Prod) ? AppConstants.AppSettingsProdFile : AppConstants.AppSettingsDevlFile, 
//     false, 
//     true)
// .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // AppSettings appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
        // services.AddSingleton(appSettings);

        services.AddSingleton<IDashCamVideoService, DashCamVideoService>();
        services.AddSingleton<IFfmpeg, Ffmpeg>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IHandymanSubtitleService, HandymanSubtitleService>();
        services.AddSingleton<IHandymanVideoService, HandymanVideoService>();
        services.AddSingleton<IMusicService, MusicService>();
        services.AddSingleton<ITarball, Tarball>();
        services.AddSingleton<IToastmastersVideoService, ToastmastersVideoService>();
        services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));

        services.AddHostedService<DashCamVideoWorker>();
        services.AddHostedService<HandymanSubtitleWorker>();
        services.AddHostedService<HandymanVideoWorker>();
        services.AddHostedService<TechnologyVideoWorker>();
        services.AddHostedService<ToastmastersVideoWorker>();
    })
    .UseSystemd()
    .Build();

await host.RunAsync();

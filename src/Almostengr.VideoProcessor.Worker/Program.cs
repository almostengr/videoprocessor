using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Videos.Services;
using Almostengr.VideoProcessor.Domain.Subtitles.Services;
using Almostengr.VideoProcessor.Worker.Workers;
using Almostengr.VideoProcessor.Infrastructure.FileSystem;
using Almostengr.VideoProcessor.Infrastructure.Logging;
using Almostengr.VideoProcessor.Infrastructure.Processes;

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
        services.AddSingleton<IFfmpegService, FfmpegService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IHandyTechSrtSubtitleService, HandyTechSrtSubtitleService>();
        services.AddSingleton<IHandyTechVideoService, HandyTechVideoService>();
        services.AddSingleton<IMusicService, MusicService>();
        services.AddSingleton<ITarballService, TarballService>();
        services.AddSingleton<IToastmastersVideoService, ToastmastersVideoService>();
        services.AddSingleton(typeof(IVpLogger<>), typeof(VpLogger<>));

        services.AddHostedService<DashCamVideoWorker>();
        services.AddHostedService<HandyTechSubtitleWorker>();
        services.AddHostedService<HandyTechVideoWorker>();
        services.AddHostedService<ToastmastersVideoWorker>();
    })
    .UseSystemd()
    .Build();

await host.RunAsync();

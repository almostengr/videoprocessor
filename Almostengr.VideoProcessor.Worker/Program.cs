using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Videos.Services;
using Almostengr.VideoProcessor.Domain.Subtitles.Services;
using Almostengr.VideoProcessor.Worker.Workers;

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
        services.AddSingleton<IHandyTechVideoService, HandyTechVideoService>();
        services.AddSingleton<IMusicService, MusicService>();
        services.AddSingleton<IHandyTechSrtSubtitleService, HandyTechSrtSubtitleService>();

        services.AddHostedService<DashCamVideoWorker>();
        services.AddHostedService<HandyTechSubtitleWorker>();
        services.AddHostedService<HandyTechVideoWorker>();
    })
    .UseSystemd()
    .Build();

await host.RunAsync();

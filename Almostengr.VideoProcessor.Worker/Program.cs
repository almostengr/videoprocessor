using Almostengr.VideoProcessor.Worker;
using Almostengr.VideoProcessor.Application.Video.Services;
using Almostengr.VideoProcessor.Application.Video;
using Almostengr.VideoProcessor.Domain.Music.Services;

// string environment = string.Empty;

// #if RELEASE
//     environment = AppEnvironment.Prod;
// #else
//     environment = AppEnvironment.Devl;
// #endif

IConfiguration configuration = new ConfigurationBuilder()
    // .AddJsonFile(
    //     (environment == AppEnvironment.Prod) ? AppConstants.AppSettingsProdFile : AppConstants.AppSettingsDevlFile, 
    //     false, 
    //     true)
    .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // AppSettings appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
        // services.AddSingleton(appSettings);

        // services.AddTransient<IDashCamVideoService, DashCamVideoService>();
        services.AddSingleton<IHandyTechVideoService, HandyTechVideoService>();
        services.AddSingleton<IMusicService, MusicService>();
        // services.AddTransient<ISrtSubtitleService, SrtSubtitleService>();

        // services.AddHostedService<DashCamVideoWorker>();
        services.AddHostedService<HandyTechSubtitleWorker>();
        services.AddHostedService<HandyTechVideoWorker>();
    })
    .Build();

await host.RunAsync();

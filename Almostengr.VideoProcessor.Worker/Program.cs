using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.Database;
using Almostengr.VideoProcessor.Core.Music;
using Almostengr.VideoProcessor.Core.Status;
using Almostengr.VideoProcessor.Core.Subtitles;
using Almostengr.VideoProcessor.Core.VideoDashCam;
using Almostengr.VideoProcessor.Core.VideoHandyTech;
using Almostengr.VideoProcessor.Worker;
using Microsoft.EntityFrameworkCore;

string environment = string.Empty;

#if RELEASE
    environment = AppEnvironment.Prod;
#else
    environment = AppEnvironment.Devl;
#endif

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile(
        (environment == AppEnvironment.Prod) ? AppConstants.AppSettingsProdFile : AppConstants.AppSettingsDevlFile, 
        false, 
        true)
    .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        AppSettings appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
        services.AddSingleton(appSettings);

        services.AddTransient<IDashCamVideoService, DashCamVideoService>();
        services.AddTransient<IHandyTechVideoService, HandyTechVideoService>();
        services.AddTransient<IMusicService, MusicService>();
        services.AddTransient<ISrtSubtitleService, SrtSubtitleService>();
        services.AddTransient<IStatusService, StatusService>();

        // services.AddSingleton<IStatusRepository, StatusRepository>();
        services.AddTransient<IStatusRepository, MockStatusRepository>();

        // services.AddDbContext<IVideoDbContext, VideoDbContext>();
        services.AddDbContext<VideoDbContext>(options => options.UseInMemoryDatabase("VideoProcessor"));

        services.AddHostedService<DashCamVideoWorker>();
        services.AddHostedService<HandyTechSubtitleWorker>();
        services.AddHostedService<HandyTechVideoWorker>();
    })
    .Build();

await host.RunAsync();

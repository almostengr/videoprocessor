using Almostengr.VideoProcessor.Core.Database;
using Almostengr.VideoProcessor.Core.Repository;
using Almostengr.VideoProcessor.Core.Services.Data;
using Almostengr.VideoProcessor.Core.Services.ExternalProcess;
using Almostengr.VideoProcessor.Core.Services.FileSystem;
using Almostengr.VideoProcessor.Core.Services.MusicService;
using Almostengr.VideoProcessor.Core.Services.Subtitles;
using Almostengr.VideoProcessor.Core.Services.Video;
using Almostengr.VideoProcessor.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // SERVICES //////////////////////////////////////////////////////////////////////////////////////////

        services.AddTransient<IDashCamVideoService, DashCamVideoService>();
        services.AddTransient<IExternalProcessService, ExternalProcessService>();
        services.AddTransient<IFileSystemService, FileSystemService>();
        services.AddTransient<IHandyTechVideoService, HandyTechVideoService>();
        services.AddTransient<IMusicService, MusicService>();
        services.AddTransient<ISrtSubtitleService, SrtSubtitleService>();
        services.AddTransient<IStatusService, StatusService>();

        // REPOSITORIES //////////////////////////////////////////////////////////////////////////////////////

        services.AddTransient<IStatusRepository, StatusRepository>();

        // DATABASE //////////////////////////////////////////////////////////////////////////////////

        services.AddDbContext<IVideoDbContext, VideoDbContext>();

        // WORKERS ///////////////////////////////////////////////////////////////////////////////////////////

        services.AddHostedService<DashCamVideoWorker>();
        services.AddHostedService<HandyTechSubtitleWorker>();
        services.AddHostedService<HandyTechVideoWorker>();
    })
    .Build();

await host.RunAsync();

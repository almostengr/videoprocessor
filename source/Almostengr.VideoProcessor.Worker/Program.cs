using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.FileSystem;
using Almostengr.VideoProcessor.Infrastructure.Logging;
using Almostengr.VideoProcessor.Infrastructure.Processes;
using Almostengr.VideoProcessor.Worker.Workers;
using Almostengr.VideoProcessor.Infrastructure.Common;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.Videos;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.Sources.Clear();
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton(new AppSettings());

        services.AddSingleton<ICsvGraphicsFileService, CsvGraphicsFileService>();
        services.AddSingleton<IFfmpegService, FfmpegService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IFileCompressionService, XzService>();
        services.AddSingleton<IGzFileCompressionService, GzipService>();
        services.AddSingleton<IXzFileCompressionService, XzService>();
        services.AddSingleton<IMusicService, MusicService>();
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<ITarballService, TarballService>();
        services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));
        services.AddSingleton<IProcessVideoService, ProcessVideoService>();

        services.AddSingleton<ISrtSubtitleFileService, SrtSubtitleFileService>();

        services.AddHostedService<VideoWorker>();
    })
    .UseSystemd()
    .Build();

await host.RunAsync();

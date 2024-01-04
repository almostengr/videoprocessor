using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.FileSystem;
using Almostengr.VideoProcessor.Infrastructure.Logging;
using Almostengr.VideoProcessor.Infrastructure.Common;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.Sources.Clear();
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton(new AppSettings());

        services.AddSingleton<ICsvGraphicsFileService, CsvGraphicsFileService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));

        services.AddSingleton<ISrtSubtitleFileService, SrtSubtitleFileService>();
    })
    .UseSystemd()
    .Build();

await host.RunAsync();

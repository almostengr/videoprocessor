using Almostengr.VideoProcessor.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<DashCamVideoWorker>();
        services.AddHostedService<HandyTechSubtitleWorker>();
        services.AddHostedService<HandyTechVideoWorker>();
    })
    .Build();

await host.RunAsync();

using Almostengr.VideoProcessor.Domain.Videos.HandymanVideo;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class HandymanVideoWorker : BaseWorker
{
    private readonly IHandymanVideoService _videoService;

    public HandymanVideoWorker(IHandymanVideoService videoService)
    {
        _videoService = videoService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
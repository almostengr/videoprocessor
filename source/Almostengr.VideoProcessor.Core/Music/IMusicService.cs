namespace Almostengr.VideoProcessor.Core.Music.Services;

public interface IMusicService
{
    Task GenerateMixTrackAsync(CancellationToken cancellationToken);
    AudioFile GetRandomMixTrack();
}
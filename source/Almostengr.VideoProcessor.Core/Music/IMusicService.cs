namespace Almostengr.VideoProcessor.Core.Music.Services;

public interface IMusicService
{
    AudioFile GetRandomNonMixTrack();
    AudioFile GetRandomMixTrack();
}
namespace Almostengr.VideoProcessor.Core.Music.Services;

public interface IMusicService
{
    MusicFile GetRandomNonMixTrack();
    MusicFile GetRandomMixTrack();
}
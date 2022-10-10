namespace Almostengr.VideoProcessor.Domain.Music.Services;

public interface IMusicService
{
    string? GetRandomNonMixTrack();
    string? GetRandomMusicTracks();
    string? GetRandomMixTrack();
}
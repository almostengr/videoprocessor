namespace Almostengr.VideoProcessor.Core.Music.Services;

public interface IMusicService
{
    // string GetRandomNonMixTrack();
    // string GetRandomMusicTracks();
    // string GetRandomMixTrack();
    MusicFile GetRandomNonMixTrack();
    MusicFile GetRandomMixTrack();
}
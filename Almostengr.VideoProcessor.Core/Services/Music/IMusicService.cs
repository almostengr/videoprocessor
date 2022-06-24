namespace Almostengr.VideoProcessor.Core.Services.MusicService
{
    public interface IMusicService
    {
        string GetRandomMusicTracks();
        string GetRandomMixTrack();
        string GetRandomNonMixTrack();
    }
}

namespace Almostengr.VideoProcessor.Core.Music
{
    public interface IMusicService
    {
        string GetRandomMusicTracks();
        string GetRandomMixTrack();
        string GetRandomNonMixTrack();
    }
}

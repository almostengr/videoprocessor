namespace Almostengr.VideoProcessor.Api.Services.MusicService
{
    public interface IMusicService
    {
        string GetRandomMusicTracks();
        string GetRandomMixTrack();
        string GetRandomNonMixTrack();
    }
}

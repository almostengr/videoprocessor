namespace Almostengr.VideoProcessor.Api.Services.MusicService
{
    public interface IMusicService
    {
        string GetFfmpegMusicInputList();
        string PickRandomMusicTrack();
    }
}

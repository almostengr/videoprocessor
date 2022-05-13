namespace Almostengr.VideoProcessor.Api.Services.MusicService
{
    public interface IMusicService : IBaseService
    {
        string GetFfmpegMusicInputList();
        string PickRandomMusicTrack();
    }
}

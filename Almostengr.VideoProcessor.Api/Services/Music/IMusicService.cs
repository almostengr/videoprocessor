namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IMusicService : IBaseService
    {
        string GetFfmpegMusicInputList();
        string PickRandomMusicTrack();
    }
}

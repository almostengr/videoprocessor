namespace Almostengr.VideoProcessor.Api.Services.Video
{
    public interface IDashCamVideoService : IVideoService
    {
        string GetDestinationFilter(string workingDirectory);
        string GetMajorRoadsFilter(string workingDirectory);
    }
}
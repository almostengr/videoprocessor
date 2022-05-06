namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IDashCamVideoRenderService : IBaseVideoRenderService
    {
        string GetDestinationFilter(string destinationFile);
        string GetMajorRoadsFilter(string majorRoadsFile);
    }
}
namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public interface IDashCamVideoRenderService : IBaseVideoRenderService
    {
        string GetDestinationFilter(string destinationFile);
        string GetMajorRoadsFilter(string majorRoadsFile);
    }
}
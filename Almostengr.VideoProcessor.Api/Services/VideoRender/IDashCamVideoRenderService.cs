namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public interface IDashCamVideoRenderService : IVideoRenderService
    {
        string GetDestinationFilter(string workingDirectory);
        string GetMajorRoadsFilter(string workingDirectory);
    }
}
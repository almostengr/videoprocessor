namespace Almostengr.VideoProcessor.Api.Services.VideoRender
{
    public interface IDashCamVideoRenderService : IBaseVideoRenderService
    {
        string GetDestinationFilter(string workingDirectory);
        string GetMajorRoadsFilter(string workingDirectory);
    }
}
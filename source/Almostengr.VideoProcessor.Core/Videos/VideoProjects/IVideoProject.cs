namespace Almostengr.VideoProcessor.Core.Videos;

public interface IVideoProject
{
    public string FilePath();
    public string FileName();
    public string ArchiveFilePath();
    public string UploadFilePath();
    public List<string> BrandingTextOptions();
    public string OutputFileName();
    public string ChannelBrandDrawTextFilter(string brandingText);
    FfMpegColor DrawTextFilterBackgroundColor();
    FfMpegColor DrawTextFilterTextColor();
    string ArchiveDirectory();
    string UploadDirectory();
}
namespace Almostengr.VideoProcessor.Core.Videos;

public interface IVideoProject
{
    public string FilePath();
    public string FileName();
    public string ArchiveDirectory();
    public string UploadDirectory();
    public List<string> BrandingTextOptions();
    public string VideoFileName();
    public string ChannelBrandDrawTextFilter(string brandingText);
}
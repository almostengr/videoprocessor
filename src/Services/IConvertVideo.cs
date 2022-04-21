namespace Almostengr.VideoProcessor.Services
{
    public interface IConvertVideo
    {
        void ConvertToFormat(string filePath, VideoFormat format);
    }
}
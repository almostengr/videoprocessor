using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface ISubtitleReadFileService
{
    List<SubtitleFileEntry> ReadFile(string filePath);
}

public interface ISubtitleReadWriteFileService : ISubtitleReadFileService
{
    void WriteFile(string filePath, IList<SubtitleFileEntry> subtitles);
}

public interface ISrtSubtitleFileService : ISubtitleReadWriteFileService
{
    
}

public interface IAssSubtitleFileService : ISubtitleReadFileService
{

}
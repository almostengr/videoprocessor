using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;

namespace Almostengr.VideoProcessor.Core.Music;

public sealed class AudioFile
{
    public string FilePath { get; private set; }

    public AudioFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !filePath.EndsWithIgnoringCase(FileExtension.Mp3.Value))
        {
            throw new ArgumentException("File path is not valid", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new ArgumentException("File path does not exist", nameof(filePath));
        }

        FilePath = filePath;
    }
    
    public string FileName()
    {
        return Path.GetFileName(FilePath);
    }
}
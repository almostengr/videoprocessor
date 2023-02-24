using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Music;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public sealed class VideoFile
{
    public VideoFile(string filePath)
    {
        ValidateVideoFile(filePath);

        FilePath = filePath;
        FileName = Path.GetFileName(FilePath);
        AudioFile = null;
    }

    private static void ValidateVideoFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new ArgumentException("File does not exist");
        }

        if (
            !filePath.ToLower().EndsWith(FileExtension.Avi.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.Mkv.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.Mov.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.Mp4.Value)
        )
        {
            throw new ArgumentException("File type is not correct", nameof(filePath));
        }
    }

    public VideoFile(string filePath, AudioFile audio)
    {
        ValidateVideoFile(filePath);
        FilePath = filePath;
        FileName = Path.GetFileName(FilePath);
        AudioFile = audio;
    }

    public string FilePath { get; init; }
    public string FileName { get; init; }
    public AudioFile? AudioFile { get; private set; }

    internal void SetAudioFile(AudioFile audioFile)
    {
        AudioFile = audioFile;
    }

    internal string TsOutputFileName()
    {
        return Path.GetFileNameWithoutExtension(FileName) + FileExtension.Ts.Value;
    }
}
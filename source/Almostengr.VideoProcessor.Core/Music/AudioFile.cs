using Almostengr.VideoProcessor.Core.Common.Constants;

namespace Almostengr.VideoProcessor.Core.Music
{
    public sealed class AudioFile
    {
        public AudioFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !filePath.ToLower().EndsWith(FileExtension.Mp3.Value))
            {
                throw new ArgumentException("File path is not valid", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new ArgumentException("File path does not exist", nameof(filePath));
            }

            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
        }

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
    }
}
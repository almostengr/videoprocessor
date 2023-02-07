using Almostengr.VideoProcessor.Core.Common.Constants;

namespace Almostengr.VideoProcessor.Core.Music
{
    public sealed class MusicFile
    {
        public MusicFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !filePath.ToLower().EndsWith(FileExtension.Mp3.Value))
            {
                throw new ArgumentException("File path is not valid", nameof(filePath));
            }

            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
        }

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
    }
}
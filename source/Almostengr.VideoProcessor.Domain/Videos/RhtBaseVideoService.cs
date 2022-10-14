using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Videos;

public abstract class RhtBaseVideoService : BaseVideoService, IBaseVideoService
{
    private readonly IFileSystem _fileSystem;

    public RhtBaseVideoService(IFileSystem fileSystem, IFfmpeg ffmpeg) : base(fileSystem, ffmpeg)
    {
        _fileSystem = fileSystem;
    }

    internal override void CreateFfmpegInputFile<T>(T video)
    {
        _fileSystem.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
                .Where(x => x.EndsWith(FileExtension.Ts))
                .OrderBy(x => x)
                .ToArray();

            const string rhtservicesintro = "rhtservicesintro.ts";
            const string file = "file";
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                if (filesInDirectory[i].Contains(rhtservicesintro))
                {
                    continue;
                }

                if (i == 1 && video.Title.ToLower().Contains(Constants.ChristmasLightShow) == false)
                {
                    writer.WriteLine($"{file} '{rhtservicesintro}'");
                }

                writer.WriteLine($"{file} '{Path.GetFileName(filesInDirectory[i])}'");
            }
        }
    }
}
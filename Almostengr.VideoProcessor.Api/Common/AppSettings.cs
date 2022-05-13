using System;

namespace Almostengr.VideoProcessor.Api.Common
{
    public class AppSettings
    {
#if RELEASE
        public TimeSpan WorkerServiceInterval { get; set; } = TimeSpan.FromHours(2);
#else
        public TimeSpan WorkerServiceInterval { get; set; } = TimeSpan.FromSeconds(30);
#endif

        public Directories Directories { get; set; } = new();
        public ProgramPaths ProgramPaths { get; set; } = new();
        public int ThumbnailFrames { get; set; } = 5;
        public bool DoRenderVideos { get; set; } = false;
        public double DiskSpaceThreshold { get; set; } = 0.05;
    }

    public class Directories
    {
#if RELEASE
        public string TranscriptBaseDirectory = "/home/almostengineer/Videos/";
        public string DashCamBaseDirectory = "/home/almostengineer/";
        public string RhtBaseDirectory = "/home/almostengineer/";
        public string MusicDirectory = "/home/almostengineer/Music/";
#else
        public string TranscriptBaseDirectory = "/home/almostengineer/Downloads/";
        public string DashCamBaseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Kenny Ram Dash Cam";
        public string RhtBaseDirectory = "/home/almostengineer/Downloads/";
        public string MusicDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/07music";
#endif
    }

    public class ProgramPaths
    {
        public const string TarBinary = "/bin/tar";
        public const string FfmpegBinary = "/usr/bin/ffmpeg";
        public const string BashShell = "/bin/bash";
    }
}
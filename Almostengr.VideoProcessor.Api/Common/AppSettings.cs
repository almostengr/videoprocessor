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
    }

    public class Directories
    {
#if RELEASE
            public string BaseDirectory = "/home/almostengineer/Videos/";
#else
        public string BaseDirectory = "/home/almostengineer/Downloads/";
#endif
    }

    public class ProgramPaths
    {
        public const string TarBinary = "/bin/tar";
        public const string FfmpegBinary = "/usr/bin/ffmpeg";
    }

}
using System;

namespace Almostengr.VideoProcessor.Api.Configuration
{
    public class AppSettings
    {
#if RELEASE
        public TimeSpan WorkerServiceInterval = TimeSpan.FromHours(2);
#else
        public TimeSpan WorkerServiceInterval = TimeSpan.FromSeconds(30);
#endif

        public Directories Directories = new();
        public readonly int ThumbnailFrames = 5;
        public readonly bool DoRenderVideos = false;
        public readonly double DiskSpaceThreshold = 0.05;
    }

    public class Directories
    {
#if RELEASE
        public readonly string TranscriptBaseDirectory = "/home/almostengineer/Videos/";
        public readonly string DashCamBaseDirectory = "/home/almostengineer/";
        public readonly string RhtBaseDirectory = "/home/almostengineer/";
        public readonly string MusicDirectory = "/home/almostengineer/Music/";
#else
        public readonly string TranscriptBaseDirectory = "/home/almostengineer/Downloads/transcripts";
        public readonly string DashCamBaseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Kenny Ram Dash Cam";
        public readonly string RhtBaseDirectory = "/home/almostengineer/Downloads/rhtvideos/";
        public readonly string MusicDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/07music";
#endif
    }
}
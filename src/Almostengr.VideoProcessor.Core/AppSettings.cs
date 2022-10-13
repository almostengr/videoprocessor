namespace Almostengr.VideoProcessor.Core.Configuration
{
    public class AppSettings
    {
        public int WorkerIdleInterval { get; set; }
        public Directories Directories { get; set; }
        public int ThumbnailFrames { get; set; }
        public bool DoRenderVideos { get; set; }
        public double DiskSpaceThreshold { get; set; }
    }

    public class Directories
    {
        public string DashCamBaseDirectory { get; set; }
        public string RhtBaseDirectory { get; set; }
        public string MusicDirectory { get; set; }
        public string IntroVideoPath { get; set; }
    }
}
namespace Almostengr.VideoProcessor.Api.Configuration
{
    public class AppSettings
    {
        public int WorkerServiceInterval { get; set; }
        public Directories Directories { get; set; }
        public int ThumbnailFrames { get; set; }
        public bool DoRenderVideos { get; set; }
        public double DiskSpaceThreshold { get; set; }
    }

    public class Directories
    {
        public string TranscriptBaseDirectory { get; set; }
        public string DashCamBaseDirectory { get; set; }
        public string RhtBaseDirectory { get; set; }
        public string MusicDirectory { get; set; }
    }
}
namespace Almostengr.VideoProcessor.Api.Configuration
{
    public class AppSettings
    {
        public int WorkerServiceInterval { get; set; }
        public Directories Directories = new();
        public int ThumbnailFrames { get; set; }
        public bool DoRenderVideos { get; set; }
        public double DiskSpaceThreshold { get; set; }
    }

    public class Directories
    {
        public string TranscriptBaseDirectory = "/home/almostengineer/Downloads/transcripts";
        public string DashCamBaseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Kenny Ram Dash Cam";
        public string RhtBaseDirectory = "/home/almostengineer/Downloads/rhtvideos/";
        public string MusicDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/07music";
    }
}
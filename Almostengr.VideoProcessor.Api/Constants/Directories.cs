namespace Almostengr.VideoProcessor.Constants
{
    public class Directories
    {
        # if RELEASE
        public const string BaseDirectory = "/home/almostengineer/Videos/";
        # else
        public const string BaseDirectory = "/home/almostengineer/Downloads/";
        # endif
    }
}
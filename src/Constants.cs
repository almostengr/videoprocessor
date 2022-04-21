namespace Almostengr.VideoProcessor
{
    public static class PrivacyStatus
    {
        public const string Public = "public";
        public const string Unlisted = "unlisted";
        public const string Private = "private";
    }

    public static class ClientSecretFileName
    {
        public const string RhtServices = "rhts_secret.json";
        public const string Dashcam = "krdc_secret.json";
    }

    public static class VideoDescription
    {
        public const string RhtServices = "Visit https://rhtservices.net for more information.";
        public const string Dashcam = "";
    }

    public static class UploadDirectory
    {
        public const string Dashcam = "/home/almostengineer/Videos/dashcam_uploads";
        public const string RhtServices = "/home/almostengineer/Videos/services_uploads";
    }

}
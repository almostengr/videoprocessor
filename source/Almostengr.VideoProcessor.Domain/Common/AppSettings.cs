namespace Almostengr.VideoProcessor.Domain.Common;

public sealed class AppSettings
{
    public string ChristmasLightShowDirectory { get; init; }
    public string DashCamDirectory { get; init; }
    public string HandymanDirectory { get; init; }
    public string TechnologyDirectory { get; init; }
    public string ToastmastersDirectory { get; init; }

    public TimeSpan WorkerDelay { get; init; }
    public double DiskSpaceThreshold { get; init; }
    public int DeleteFilesAfterDays { get; init; }
    public string RhtServicesIntroPath { get; init; }

    public AppSettings()
    {
        if (IsReleaseMode())
        {
            ChristmasLightShowDirectory = string.Empty;
            DashCamDirectory = string.Empty;
            DeleteFilesAfterDays = 30;
            DiskSpaceThreshold = 0.1;
            HandymanDirectory = string.Empty;
            RhtServicesIntroPath = string.Empty;
            TechnologyDirectory = string.Empty;
            ToastmastersDirectory = string.Empty;
            WorkerDelay = TimeSpan.FromMinutes(120);
            return;
        }

        const string baseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3";
        ChristmasLightShowDirectory = $"{baseDirectory}/ChristmasLightShow";
        DashCamDirectory = $"{baseDirectory}/Kenny Ram Dash Cam";
        DeleteFilesAfterDays = 0; // 0 to disable
        DiskSpaceThreshold = 0; // 0.0 to disable
        HandymanDirectory = $"{baseDirectory}/RhtHandyman";
        RhtServicesIntroPath = $"{baseDirectory}/ytvideostructure/rhtservicesintro.mp4";
        TechnologyDirectory = $"{baseDirectory}/RhtTechnology";
        ToastmastersDirectory = $"{baseDirectory}/Toastmasters";
        WorkerDelay = TimeSpan.FromSeconds(15);
    }

    public bool IsReleaseMode()
    {
#if RELEASE
            return true;
#else
        return false;
# endif
    }
}

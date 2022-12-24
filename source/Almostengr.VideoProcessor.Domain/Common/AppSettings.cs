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
            const string pbaseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3";
            ChristmasLightShowDirectory = $"{pbaseDirectory}/ChristmasLightShow";
            DashCamDirectory = $"{pbaseDirectory}/";
            DeleteFilesAfterDays = 30;
            DiskSpaceThreshold = 0.1;
            HandymanDirectory = $"{pbaseDirectory}/RhtHandyman";
            RhtServicesIntroPath = string.Empty;
            TechnologyDirectory = $"{pbaseDirectory}/RhtTechnology";
            ToastmastersDirectory = $"{pbaseDirectory}/Toastmasters";
            WorkerDelay = TimeSpan.FromMinutes(120);
            return;
        }

        const string baseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3";
        ChristmasLightShowDirectory = $"{baseDirectory}/ChristmasLightShow";
        DashCamDirectory = $"{baseDirectory}/Kenny Ram Dash Cam";
        DeleteFilesAfterDays = 0; // 0 to disable
        DiskSpaceThreshold = 0; // 0.0 to disable
        HandymanDirectory = $"{baseDirectory}/RhtHandyman";
        RhtServicesIntroPath = $"{baseDirectory}/ytvideostructure/rhtservicesintro.ts";
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

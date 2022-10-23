namespace Almostengr.VideoProcessor.Domain.Common;

public sealed class AppSettings
{
    public string ChristmasLightShowDirectory { get; init; }
    public string DashCamDirectory { get; init; }
    public string HandymanDirectory { get; init; }
    public string TechnologyDirectory { get; init; }
    public string ToastmastersDirectory { get; init; }

    public int WorkerDelayMinutes { get; init; }
    public double DiskSpaceThreshold { get; init; }

    public AppSettings()
    {
        if (IsReleaseMode())
        {
            ChristmasLightShowDirectory = string.Empty;
            DashCamDirectory = string.Empty;
            DashCamDirectory = string.Empty;
            DiskSpaceThreshold = 0.1;
            HandymanDirectory = string.Empty;
            TechnologyDirectory = string.Empty;
            ToastmastersDirectory = string.Empty;
            WorkerDelayMinutes = 120;
            return;
        }

        ChristmasLightShowDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ChristmasLightShow";
        DashCamDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Kenny Ram Dash Cam";
        DiskSpaceThreshold = 0.01;
        HandymanDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/RhtHandyman";
        TechnologyDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/RhtTechnology";
        ToastmastersDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Toastmasters";
        WorkerDelayMinutes = 1;
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

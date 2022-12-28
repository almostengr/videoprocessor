namespace Almostengr.VideoProcessor.Domain.Common;

public sealed class AppSettings
{
    public string DashCamDirectory { get; init; }
    public string HandymanDirectory { get; init; }
    public string TechnologyDirectory { get; init; }
    public string ToastmastersDirectory { get; init; }
    public TimeSpan WorkerDelay { get; init; }
    public double DiskSpaceThreshold { get; init; }
    public int DeleteFilesAfterDays { get; init; }

    public AppSettings()
    {
        if (IsReleaseMode())
        {
            const string pbaseDirectory = "/mnt/3761e00d-e29b-4073-b282-589ade503755";
            DashCamDirectory = $"{pbaseDirectory}/dashcam";
            DeleteFilesAfterDays = 30;
            DiskSpaceThreshold = 0.1;
            HandymanDirectory = $"{pbaseDirectory}/handyman";
            TechnologyDirectory = $"{pbaseDirectory}/technology";
            ToastmastersDirectory = $"{pbaseDirectory}/toastmasters";
            WorkerDelay = TimeSpan.FromMinutes(120);
            return;
        }

        const string baseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3";
        DashCamDirectory = $"{baseDirectory}/dashcam";
        DeleteFilesAfterDays = 0; // 0 to disable
        DiskSpaceThreshold = 0; // 0.0 to disable
        HandymanDirectory = $"{baseDirectory}/handyman";
        TechnologyDirectory = $"{baseDirectory}/technology";
        ToastmastersDirectory = $"{baseDirectory}/toastmasters";
        WorkerDelay = TimeSpan.FromSeconds(15);
    }

    private bool IsReleaseMode()
    {
#if RELEASE
        return true;
# endif

        return false;
    }
}

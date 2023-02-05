namespace Almostengr.VideoProcessor.Core.Constants;

public static class DirectoryName
{
    // public static readonly string Rendering = "rendering";
    // public static readonly string Draft = "draft";
    // public static readonly string Working = "working";

    // incoming (tarballs) -> production (graphics and audio edited) -> upload
    // incomingwork
    // productionwork
    // archive

    // public static readonly string Production = "production";
    // public static readonly string IncomingWork = "incomingwork";
    // public static readonly string ProductionWork = "productionwork";

    // incoming extracts to assembling; tarballs move to archive
    // assembling renders to reviewing
    // reviewing renders to producing; subtitles move to archive
    // producing moves to upload
    public static readonly string Incoming = "incoming";
    // public static readonly string Assembling = "assembling";
    // public static readonly string Assembled = "assembled";
    // public static readonly string Reviewing = "reviewing";
    // public static readonly string Reviewed = "reviewed";
    // public static readonly string Producing = "producing";
    public static readonly string Archive = "archive";
    public static readonly string Uploading = "uploading";

    public static readonly string ReviewWork = "reviewwork";
    public static readonly string IncomingWork = "incomingwork";
    public static readonly string Reviewing = "reviewing";

    // received, assembling, assembled, reviewing, reviewed, uploading


    // received, rendering, rendered, animating, animated, uploading


}

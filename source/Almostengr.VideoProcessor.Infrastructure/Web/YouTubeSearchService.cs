using Almostengr.VideoProcessor.Core.Common;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace Almostengr.VideoProcessor.Infrastructure.Web;
public class YouTubeSearchService : IYouTubeSearchService
{
    private readonly YouTubeService _youtubeService;
    // private readonly string _youtubeApiKey;

    public YouTubeSearchService(AppSettings appSettings)
    {
        // _youtubeApiKey = configuration.GetValue<string>("YouTubeApiKey") ?? string.Empty;
        _youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            // ApiKey = _youtubeApiKey,
            ApiKey = appSettings.YouTubeApiKey,
            // ApplicationName = "MyProject"
            ApplicationName = appSettings.AppName
        });
    }

    public string SearchKeywords(string videoTitle)
    {
        if (string.IsNullOrWhiteSpace(videoTitle))
        {
            throw new ArgumentException("Video title cannot be null or whitespace.", nameof(videoTitle));
        }

        var searchListRequest = _youtubeService.Search.List("snippet");
        searchListRequest.Q = videoTitle;
        searchListRequest.MaxResults = 1;

        var searchListResponse = searchListRequest.Execute();

        if (searchListResponse.Items.Count == 0)
        {
            return string.Empty;
        }

        var video = searchListResponse.Items.First();

        // return string.Join(",", video.Snippet.Tags); // todo
        return string.Empty;
    }
}
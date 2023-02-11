using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Handyman;
using Almostengr.VideoProcessor.Core.TechTalk;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Almostengr.VideoProcessor.Infrastructure.Web;

public sealed class ThumbnailService : IThumbnailService
{
    private readonly ILoggerService<ThumbnailService> _logger;

    public ThumbnailService(ILoggerService<ThumbnailService> logger)
    {
        _logger = logger;
    }

    public void GetThumbnail()
    {

    }

    public void GetThumbnails<T>(T videoFile, IEnumerable<string> titles) where T : BaseVideoFile
    {
        string url = string.Empty;
        switch (videoFile)
        {
            case TechTalkVideoFile:
                url = "https://rhtservices.net/tntechtalk.html?videoTitle=";
                break;

            case HandymanVideoFile:
                url = "https://rhtservices.net/tntechtalk.html?videoTitle=";
                break;

            default:
                throw new ArgumentException("Invalid video file type", nameof(videoFile));
        }

        IWebDriver? driver = null;

        try
        {
            driver = new ChromeDriver();

            foreach (var title in titles)
            {
                driver.Navigate().GoToUrl(url + title);

                // take screenshot
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        if (driver != null)
        {
            driver.Quit();
        }

    }

}

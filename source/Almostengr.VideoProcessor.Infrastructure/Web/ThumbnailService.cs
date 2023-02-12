using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Almostengr.VideoProcessor.Infrastructure.Web;

public sealed class ThumbnailService : IThumbnailService
{
    private readonly ILoggerService<ThumbnailService> _logger;
    private readonly AppSettings _appSettings;

    public ThumbnailService(ILoggerService<ThumbnailService> logger, AppSettings appsettings)
    {
        _logger = logger;
        _appSettings = appsettings;
    }

    public void GenerateThumbnail(ThumbnailType type, string uploadDirectory, string thumbnailFileName, string title)
    {
        string webpageFileName = string.Empty;

        switch(type)
        {
            case ThumbnailType.TechTalk:
                webpageFileName = "tntechtalk.html";
                break;

            case ThumbnailType.Handyman:
                webpageFileName = "tnhandyman.html";
                break;

            default:
                throw new ArgumentException("invalid video type", nameof(type));
        }

        IWebDriver? driver = null;

        try
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--headless");
            driver = new ChromeDriver(_appSettings.ChromeDriverPath, options);

            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl($"https://rhtservices.net/{webpageFileName}?videoTitle={title}");

            Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(Path.Combine(uploadDirectory, thumbnailFileName));

            QuitBrowser(driver);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            QuitBrowser(driver);
            throw;
        }
    }

    private void QuitBrowser(IWebDriver? driver)
    {
        if (driver != null)
        {
            driver.Quit();
        }
    }

}

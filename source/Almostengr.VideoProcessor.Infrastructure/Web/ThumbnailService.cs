using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Almostengr.VideoProcessor.Infrastructure.Web;

public sealed class ThumbnailService : IThumbnailService
{
    private readonly AppSettings _appSettings;
    private IWebDriver _driver;

    public ThumbnailService(ILoggerService<ThumbnailService> logger, AppSettings appsettings)
    {
        _appSettings = appsettings;

        _driver = new ChromeDriver(_appSettings.ChromeDriverPath, BrowserOptions());
    }

    ~ThumbnailService()
    {
        QuitBrowser(_driver);
    }

    private ChromeOptions BrowserOptions()
    {
        ChromeOptions options = new();
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--headless");
        return options;
    }

    public void GenerateThumbnail<T>(string uploadDirectory, T thumbnailFile) where T : BaseThumbnailFile
    {
        try
        {
            _driver.Manage().Window.Maximize();
            _driver.Navigate().GoToUrl($"https://rhtservices.net/{thumbnailFile.WebPageFileName()}?videoTitle={thumbnailFile.Title()}");

            Screenshot screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
            screenshot.SaveAsFile(Path.Combine(uploadDirectory, thumbnailFile.ThumbnailFileName()));
        }
        catch (Exception)
        {
            QuitBrowser(_driver);
            throw;
        }
    }

    public void GenerateThumbnails<T>(string uploadDirectory, IEnumerable<T> thumbnailFiles) where T : BaseThumbnailFile
    {
        try
        {
            foreach (T thumbnailFile in thumbnailFiles)
            {
                _driver.Navigate().GoToUrl($"https://rhtservices.net/{thumbnailFile.WebPageFileName()}?videoTitle={thumbnailFile.Title()}");

                Screenshot screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                screenshot.SaveAsFile(Path.Combine(uploadDirectory, thumbnailFile.ThumbnailFileName()));
            }
        }
        catch (Exception)
        {
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

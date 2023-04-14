using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Almostengr.VideoProcessor.Infrastructure.Web;

public sealed class ThumbnailService : IThumbnailService
{
    private readonly AppSettings _appSettings;

    public ThumbnailService(ILoggerService<ThumbnailService> logger, AppSettings appsettings)
    {
        _appSettings = appsettings;
    }

    public void GenerateThumbnails<T>(string uploadDirectory, IEnumerable<T> thumbnailFiles) where T : BaseThumbnailFile
    {
        IWebDriver? driver = null;

        try
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--headless");

            driver = new ChromeDriver(_appSettings.ChromeDriverPath, options);
            driver.Manage().Window.Maximize();

            foreach (T thumbnailFile in thumbnailFiles)
            {
                driver.Navigate().GoToUrl($"https://rhtservices.net/{thumbnailFile.WebPageFileName()}?videoTitle={thumbnailFile.Title()}");

                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(Path.Combine(uploadDirectory, thumbnailFile.ThumbnailFileName()));
            }

            QuitBrowser(driver);
        }
        catch (Exception)
        {
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

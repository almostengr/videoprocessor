using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Reflection;
using Google.Apis.Auth.OAuth2;
using System.IO;

namespace Almostengr.VideoProcessor.Workers
{
    public class Uploadworker : BackgroundService
    {
        private readonly ILogger<Uploadworker> _logger;

        public Uploadworker(ILogger<Uploadworker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string[] videoFiles = Directory.GetFiles(UploadDirectory.Dashcam, "*.mp4"); // get the video files ready for upload

                foreach (string file in videoFiles)
                {
                    string videoTitle = GetVideoTitleFromFileName(file); // get video name from file name

                    await PerformVideoUploadAsync(
                        ClientSecretFileName.Dashcam,
                        file,
                        videoTitle,
                        VideoDescription.Dashcam
                        );
                }

                videoFiles = Directory.GetFiles(UploadDirectory.RhtServices, "*.mp4");

                foreach (string file in videoFiles)
                {
                    string videoTitle = GetVideoTitleFromFileName(file); // get video name from file name

                    await PerformVideoUploadAsync(
                        ClientSecretFileName.RhtServices,
                        file,
                        videoTitle,
                        VideoDescription.RhtServices
                        );
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task PerformVideoUploadAsync(string secretsFileName, string videoFilename, string videoTitle,
            string videoDescription = "", string categoryId = "22")
        {
            UserCredential credential;
            using (FileStream stream = new FileStream(secretsFileName, FileMode.Open, FileAccess.Read))
            // using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None
                );
            }

            YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });

            Video video = new Video();

            video.Snippet = new VideoSnippet();
            // video.Snippet.Title = "Default Video Title";
            video.Snippet.Title = videoTitle;
            // video.Snippet.Description = "Default Video Description";
            video.Snippet.Description = videoDescription;
            // video.Snippet.Tags = new string[] { "tag1", "tag2" };
            // video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Snippet.CategoryId = categoryId;
            video.Snippet.DefaultAudioLanguage = "en";
            
            video.RecordingDetails.Location = new GeoPoint(); 
            video.RecordingDetails.Location.Latitude = 32.37980; // default to Montgomery, AL
            video.RecordingDetails.Location.Longitude = -86.30782;

            video.MonetizationDetails = new VideoMonetizationDetails();
            video.MonetizationDetails.Access = new AccessPolicy();
            video.MonetizationDetails.Access.Allowed = true;
            
            // video.Snippet.Thumbnails = new ThumbnailDetails();
            // video.Snippet.Thumbnails.Default__ = new Thumbnail();

            video.Status = new VideoStatus();
            // video.Status.PrivacyStatus = "unlisted"; // or "private" or "public"
            video.Status.PrivacyStatus = PrivacyStatus.Private;
            video.Status.MadeForKids = false;
            video.Status.Embeddable = true;

            // var filePath = @"REPLACE_ME.mp4"; // Replace with path to actual movie file.
            string filePath = videoFilename;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                await videosInsertRequest.UploadAsync();
            }
        }

        private string GetVideoTitleFromFileName(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    _logger.LogInformation("{0} bytes sent.", progress.BytesSent);
                    break;

                case UploadStatus.Failed:
                    _logger.LogError("An error prevented the upload from completing.\n{0}", progress.Exception);
                    break;
            }
        }

        void videosInsertRequest_ResponseReceived(Video video)
        {
            _logger.LogInformation("Video id '{0}' was successfully uploaded.", video.Id);
        }
    }
}

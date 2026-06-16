using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace Almostengr.YtUploader;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // 1. Validate that Bash sent all required arguments
        if (args.Length < 3)
        {
            Console.WriteLine("Error: Missing arguments. Usage: UploaderApp <videoPath> <title> <channelName>");
            return 1; // Return error code to Bash
        }

        string videoPath = args[0];
        string videoTitle = args[1];
        string channelName = args[2]; // e.g., "channel1"

        // 2. Dynamically resolve the secrets and credentials file paths
        string secretsFile = $"{channelName}_secrets.json";
        string credsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".credentials_{channelName}");

        if (!File.Exists(secretsFile))
        {
            Console.WriteLine($"Error: Secrets file '{secretsFile}' not found!");
            return 1;
        }

        try
        {
            UserCredential credential;
            using (var stream = new FileStream(secretsFile, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(credsFolder, true)
                );
            }

            // 4. Create the YouTube Service and run the upload
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DashcamAutomationUploader"
            });

            var video = new Google.Apis.YouTube.v3.Data.Video
            {
                Snippet = new Google.Apis.YouTube.v3.Data.VideoSnippet()
                {
                    Title = folderName,
                    Description = "Recorded drive.",
                    CategoryId = "26" // Vehicles
                },
                Status = new Google.Apis.YouTube.v3.Data.VideoStatus() { PrivacyStatus = "public" }
            };

            using var fileStream = new FileStream(finalVideoPath, FileMode.Open);
            var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
            videosInsertRequest.ProgressChanged += (p) => Console.WriteLine($"Status: {p.Status}");
            await videosInsertRequest.UploadAsync();

            Console.WriteLine("Video uploaded successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 1;
        }
    }
}

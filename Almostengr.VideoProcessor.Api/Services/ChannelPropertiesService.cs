using System.Collections.Generic;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Constants;

namespace Almostengr.VideoProcessor.Api.Services
{
    public class ChannelPropertiesService : IChannelPropertiesService
    {
        private readonly string _baseDirectory = Directories.BaseDirectory;

        public ChannelPropertiesDto GetChannelProperties(string channelName)
        {
            ChannelPropertiesDto channelProperties = null;

            switch (channelName)
            {
                case ChannelName.DashCam:
                    channelProperties = new ChannelPropertiesDto
                    {
                        Name = channelName,
                        InputDirectory = _baseDirectory + "/dashcam/input",
                        WorkingDirectory = _baseDirectory + "/dashcam/working",
                        ArchiveDirectory = _baseDirectory + "/dashcam/archive",
                        UploadDirectory = _baseDirectory + "/dashcam/upload",
                        ClientSecretFileName = _baseDirectory + "dashcam_secrets.json",
                        DefaultDescription = "",
                        ChannelLabels = new List<string> {
                            "Kenny Ram Dash Cam"
                        }
                    };
                    break;

                case ChannelName.RhtServices:
                    channelProperties = new ChannelPropertiesDto
                    {
                        Name = channelName,
                        InputDirectory = _baseDirectory + "/rhtservices/input",
                        WorkingDirectory = _baseDirectory + "/rhtservices/working",
                        ArchiveDirectory = _baseDirectory + "/rhtservices/archive",
                        UploadDirectory = _baseDirectory + "/rhtservices/upload",
                        ClientSecretFileName = _baseDirectory + "rhtservices_secrets.json",
                        DefaultDescription = "Visit https://rhtservices.net for more information.",
                        ChannelLabels = new List<string> {
                            "Robinson Handy and Technology Services",
                            "rhtservices.net",
                            "IG: @rhtservicesllc",
                            "facebook.com/rhtservicesllc",
                            "Subscribe to our YouTube channel!",
                        }
                    };
                    break;

                default:
                    break;
            }

            return channelProperties;
        }
    }
}
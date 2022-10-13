using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Status
{
    public class StatusDto : BaseDto
    {
        public StatusKeys Key { get; set; }

        public string KeyDescription
        {
            get { return ((StatusKeys)Key).ToString(); }
        }

        public string Value { get; set; }
        public DateTime LastChanged { get; set; }
    }
}
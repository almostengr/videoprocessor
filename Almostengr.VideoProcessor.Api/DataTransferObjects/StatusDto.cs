using System;

namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class StatusDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
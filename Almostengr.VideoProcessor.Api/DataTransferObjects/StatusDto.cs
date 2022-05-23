using System;
using Almostengr.VideoProcessor.Api.Enums;

namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class StatusDto
    {
        public StatusKeys Key { get; set; }
        public string Value { get; set; }
        public DateTime LastChanged { get; set; }
    }
}
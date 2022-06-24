using System;
using Almostengr.VideoProcessor.Core.Enums;

namespace Almostengr.VideoProcessor.Core.DataTransferObjects
{
    public class StatusDto
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
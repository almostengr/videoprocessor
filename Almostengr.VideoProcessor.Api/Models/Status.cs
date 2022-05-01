using System;

namespace Almostengr.VideoProcessor.Api.Models
{
    public class Status
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
using System;
using Almostengr.VideoProcessor.Core.DataTransferObjects;
using Almostengr.VideoProcessor.Core.Enums;

namespace Almostengr.VideoProcessor.Core.Models
{
    public class Status : BaseModel
    {
        public Status()
        { }

        public Status(StatusDto statusDto)
        {
            this.Id = (int)statusDto.Key;
            this.Value = statusDto.Value;
            this.LastChanged = DateTime.Now;
        }

        public string Value { get; set; }
        public DateTime LastChanged { get; set; }

        internal StatusDto AsDto()
        {
            return new StatusDto
            {
                Key = (StatusKeys)this.Id,
                Value = this.Value,
                LastChanged = this.LastChanged
            };
        }

    }
}

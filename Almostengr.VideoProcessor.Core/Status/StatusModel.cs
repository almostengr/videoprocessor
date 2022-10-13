using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Status
{
    public sealed class StatusModel : BaseModel
    {
        public StatusModel()
        { }

        public StatusModel(StatusDto statusDto)
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

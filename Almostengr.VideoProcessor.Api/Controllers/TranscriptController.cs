using Almostengr.VideoProcessor.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Controllers
{
    public class TranscriptController : ControllerBase
    {
        private readonly ILogger<TranscriptController> _logger;
        private readonly ITranscriptService _transcriptService;

        public TranscriptController(ILogger<TranscriptController> logger, ITranscriptService transcripService)
        {
            _logger = logger;
            _transcriptService = transcripService;
        }

        [HttpPost]
        public ActionResult<TranscriptOutputDto> CleanTranscript(TranscriptInputDto inputDto)
        {
            if (_transcriptService.IsValidTranscript(inputDto) == false)
            {
                string invalidMsg = "Input is not in a valid format";
                _logger.LogError(invalidMsg);
                return BadRequest(invalidMsg);
            }

            return _transcriptService.CleanTranscript(inputDto);
        }

    }
}
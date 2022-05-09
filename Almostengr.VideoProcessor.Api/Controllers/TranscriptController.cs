using Almostengr.VideoProcessor.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Controllers
{
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly ILogger<TranscriptController> _logger;
        private readonly ISubtitleService _subtitleService;

        public TranscriptController(ILogger<TranscriptController> logger, ISubtitleService subtitleService)
        {
            _logger = logger;
            _subtitleService = subtitleService;
        }

        [HttpPost]
        [Route("/transcript/clean")]
        public ActionResult<SubtitleOutputDto> CleanTranscript(SubtitleInputDto inputDto)
        {
            if (_subtitleService.IsValidTranscript(inputDto) == false)
            {
                string invalidMsg = "Input is not in a valid format";
                _logger.LogError(invalidMsg);
                return BadRequest(invalidMsg);
            }

            return _subtitleService.CleanTranscript(inputDto);
        }

    }
}
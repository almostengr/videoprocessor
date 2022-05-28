using Almostengr.VideoProcessor.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Almostengr.VideoProcessor.Api.Services.Subtitles;
using System.Threading.Tasks;

namespace Almostengr.VideoProcessor.Controllers
{
    [ApiController]
    public class SubtitleController : ControllerBase
    {
        private readonly ILogger<SubtitleController> _logger;
        private readonly ISrtSubtitleService _subtitleService;

        public SubtitleController(ILogger<SubtitleController> logger, ISrtSubtitleService subtitleService)
        {
            _logger = logger;
            _subtitleService = subtitleService;
        }

        [HttpPost]
        [Route("/subtitle/clean")]
        public async Task<IActionResult> CleanSubtitle(SubtitleInputDto inputDto)
        {
            if (_subtitleService.IsValidFile(inputDto) == false)
            {
                return BadRequest("Input is not in a valid format");
            }

            var subtitle = await Task.Run(() => _subtitleService.CleanSubtitle(inputDto));
            return Ok(subtitle);
        }

    }
}
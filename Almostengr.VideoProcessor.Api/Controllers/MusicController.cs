using System.Collections.Generic;

using Almostengr.VideoProcessor.Api.Services.MusicService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Controllers
{
    [ApiController]
    public class MusicController : ControllerBase
    {
        private readonly ILogger<MusicController> _logger;
        private readonly IMusicService _musicService;

        public MusicController(ILogger<MusicController> logger, IMusicService musicService)
        {
            _logger = logger;
            _musicService = musicService;
        }

        [HttpGet]
        [Route("/music/inputlist")]
        public ActionResult<List<string>> GetFfmpegMusicInputList()
        {
            return Ok(_musicService.GetFfmpegMusicInputList());
        }

    }
}
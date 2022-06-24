using Almostengr.VideoProcessor.Core.Services.MusicService;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetFfmpegMusicInputList()
        {
            var tracks = await Task.Run(() => _musicService.GetRandomMusicTracks());
            return Ok(tracks);
        }

    }
}
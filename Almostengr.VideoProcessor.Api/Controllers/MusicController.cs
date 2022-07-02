using Almostengr.VideoProcessor.Core.Music;
using Microsoft.AspNetCore.Mvc;

namespace Almostengr.VideoProcessor.Controllers
{
    [ApiController]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;

        public MusicController(IMusicService musicService)
        {
            _musicService = musicService;
        }

        [HttpGet]
        [Route("/music/inputlist")]
        public async Task<IActionResult> GetFfmpegMusicInputList()
        {
            return Ok(await Task.Run(() => _musicService.GetRandomMusicTracks()));
        }

    }
}
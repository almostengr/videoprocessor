using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Almostengr.VideoProcessor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetRenderingStatus()
        {
            return Ok("Rendering");
        }
    }
}
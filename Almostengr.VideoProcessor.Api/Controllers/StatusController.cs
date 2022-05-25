using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Enums;
using Almostengr.VideoProcessor.Api.Services.Data;
using Microsoft.AspNetCore.Mvc;

namespace Almostengr.VideoProcessor.Api.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _statusService;

        public StatusController(IStatusService statusService)
        {
            _statusService = statusService;
        }

        [HttpGet]
        [Route("/status/{key}")]
        public async Task<IActionResult> GetStatusByKey(StatusKeys key)
        {
            var response = await _statusService.GetByIdAsync(key);
            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }


        [HttpGet]
        [Route("/status/")]
        public async Task<IActionResult> GetStatusList()
        {
            return Ok(await _statusService.GetListAsync());
        }
    }
}
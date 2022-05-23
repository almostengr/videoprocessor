using System.Collections.Generic;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
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
        [Route("/status/")]
        public async Task<ActionResult<List<StatusDto>>> GetStatusList()
        {
            return Ok(await _statusService.GetListAsync());
        }
    }
}
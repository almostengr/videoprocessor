using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class CaptionsWorker : BackgroundService
    {
        private const string InputDirectory = "";
        private const string OutputDirectory = "";
        private readonly ILogger<CaptionsWorker> _logger;

        public CaptionsWorker(ILogger<CaptionsWorker> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // check the incoming directory for caption files 

                // process each of the files 
                
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}